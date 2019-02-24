using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json;
using fldAsuranceApi.Models;
using System.Configuration;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;
using System.Text;

namespace fldAsuranceApi.Controllers
{
    public class FieldsController : ApiController
    {
        private DBEntities db = new DBEntities();

        // GET: api/Fields
        public IEnumerable<Field> GetFields([FromUri]PagingParameterModel pagingparametermodel)
        {
            // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
            string SortOrder = pagingparametermodel.sortOrder;

            string ColumnName = pagingparametermodel.columnName;
            //WHAT FIELD IS BEING SEARCHED
            string QuerySearchName = pagingparametermodel.querySearchName;    //.ToLower();

            var source = Enumerable.Empty<Field>().AsQueryable();

            var propertyInfo = typeof(Field).GetProperty(ColumnName);
            // SORT Order
            if (SortOrder == "desc")
            {
                source = (
                from field in db.Fields.AsEnumerable().OrderByDescending(a =>
                    propertyInfo.GetValue(a, null)
                )
                select field
            ).AsQueryable();
            }
            else
            {
                source = (
                from field in db.Fields.AsEnumerable().OrderBy(a =>
                    propertyInfo.GetValue(a, null)
                )
                select field
            ).AsQueryable();
            }

            if (!string.IsNullOrEmpty(pagingparametermodel.querySearch))
            {
                var propertyQueryInfo = typeof(Field).GetProperty(pagingparametermodel.querySearchName);

                source = (source.AsEnumerable().Where(
                    a => propertyQueryInfo.GetValue(a, null).ToString().ToLower().Contains(pagingparametermodel.querySearch.ToLower())

                )).AsQueryable();
            }
            //int count = db.Products.Count();
            int count = source.Count();

            // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
            int CurrentPage = pagingparametermodel.pageNumber;

            // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
            int PageSize = pagingparametermodel.pageSize;

            // Display TotalCount to Records to User  
            int TotalCount = count;

            // Calculating Totalpage by Dividing (No of Records / Pagesize)  
            int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

            // Returns List of Customer after applying Paging   
            var items = source.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

            // if CurrentPage is greater than 1 means it has previousPage  
            var previousPage = CurrentPage > 1 ? "Yes" : "No";

            // if TotalPages is greater than CurrentPage means it has nextPage  
            var nextPage = CurrentPage < TotalPages ? "Yes" : "No";

            // Object which we are going to send in header   
            var paginationMetadata = new
            {
                totalCount = TotalCount,
                pageSize = PageSize,
                currentPage = CurrentPage,
                totalPages = TotalPages,
                previousPage,
                nextPage
            };

            // Setting Header  
 
            HttpContext.Current.Response.Headers.Add("Paging-Headers", JsonConvert.SerializeObject(paginationMetadata));
            // Returing List of Details
            return items;
            ///////////////////////////////
            //return db.Fields;
        }

        // GET: api/Fields/5
        [ResponseType(typeof(Field))]
        public async Task<IHttpActionResult> GetField(int id)
        {
            Field field = await db.Fields.FindAsync(id);
            if (field == null)
            {
                return NotFound();
            }

            return Ok(field);
        }

        // PUT: api/Fields/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutField(int id, Field field)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != field.fldid)
            {
                return BadRequest();
            }

            db.Entry(field).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FieldExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }


        [ResponseType(typeof(Field))]
        public async Task<IHttpActionResult> PostField(Field fieldRecord)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try{

                String serviceAccStr = "";
                //STEP 0 SMTP - GET SERVICE ACCOUNT CREDENTALS
                string smtpServer = ConfigurationManager.AppSettings["smtpServer"];
                String keywordValue = "SMTP Service Account";
                //find record by keyword value
                var propertyInfo = typeof(Keyword).GetProperty("Keyword1");
                var result = (
                from record in db.Keywords.AsEnumerable().Where(a =>
                propertyInfo.GetValue(a, null).ToString().ToLower().Contains(keywordValue.ToLower())
                    )
                select record

                ).AsQueryable();
                    if (result == null)
                    {
                    return Ok("Form processing error...SQL Record not found.");
                    }

                //get first record result.First()
                serviceAccStr = result.First().Value.ToString();
                //Convert comma seperated string of Groups in to an array
                String[] authGroups = serviceAccStr.Split(',');

                string emailAdmin = authGroups[0].Trim();
                string emailAdminPassword = authGroups[1].Trim();
          
                //STEP 1 - SAVE NEW RECORD
                db.Fields.Add(fieldRecord);
                await db.SaveChangesAsync();

                CreatedAtRoute("DefaultApi", new { fid = fieldRecord.fldid }, fieldRecord);
                int fid = fieldRecord.fldid;
                string langCode = fieldRecord.Lang;
                string ccRequestor = fieldRecord.SubmittedByEmail.ToString();


                // Make a list of languages
                List<string> langCodeList = new List<string>();
                langCodeList.Add("en");
                // Add to list if language selected is not english
                if(langCode != "en")
                {
                    langCodeList.Add(langCode);
                }

                //STEP 2 - PRINT TO PDF FOR ALL LANGUAGES
                //1. We have the data now write to a PDF file
                //2. Provide a list of files that were generated
                //3. If "en" one file, if other lang "en" plus other file
                //4. final product is a list of file paths for email
                List<string> filePathList = new List<string>();
                try
                {
                    //IronPdf.Installation.TempFolderPath = @"C:\ironpdftemp";
                    foreach (var eachLangCode in langCodeList)
                    {
                        String strPg = createHTMLTextForPDF(fieldRecord, eachLangCode);
                        // Console.WriteLine("Amount is {0} and type is {1}", money.amount, money.type);
                        var fileName = "FileAssurance" + eachLangCode + DateTime.Now.ToString("yymmssfff");
                        var fullFileName = fileName + ".pdf";
                        //This writes PDF to project path as root

                        var filePath = HttpContext.Current.Server.MapPath("~/PDFFileLibrary/" + fullFileName);
                        filePathList.Add(filePath);
                        //IronPdf.Installation.TempFolderPath = tmpPath;
                        var Renderer = new IronPdf.HtmlToPdf();

                        var PDF = Renderer.RenderHtmlAsPdf(strPg);
                        PDF.SaveAs(filePath);
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("IronPDF processing error " + ex.ToString());
                    //return Ok("Communication error.  Please resubmit the form.");
                    return Ok("Form processing error..." + ex.ToString());
                    //throw ex;
                    // return NotFound();
                }

                // This opens our PDF file so we can see the result
                // STEP 3. - EMAIL THIS FILE ATTACHMENT /////////////////////////////

                // ACCESS web.config file for user name and password to access office365 smtp server.
                if (emailAdmin == null || emailAdminPassword == null)
                {
                    Console.WriteLine("Web.config file missing emailAdmin or emailAdminPassword key in appSettings");
                }

                //GET LIST OF PEOPLE TO RECEIVE THE EMAIL NOTIFICATION

                string role = "reviewer";
                //var source = db.FieldWorkFlows;  //we have a count!!!!!!!!!!!!!!
                var propertyInfoRole = typeof(FieldWorkFlow).GetProperty("Role");
                var source = (
                        from field in db.FieldWorkFlows.AsEnumerable().Where(a =>
                        propertyInfoRole.GetValue(a, null).ToString().ToLower().Contains(role.ToLower())
                    )
                    select field

                ).AsQueryable();
                //CREATE MAIL MESSAGE
                using (MailMessage message = new MailMessage())
                {
                    foreach (var element in source)
                    {
                        message.To.Add(new MailAddress(element.Email.ToString()));
                    }
                    message.From = new MailAddress(emailAdmin);

                    // Add the file attachment to this e-mail message.
                    // Create  the file attachment for this e-mail message.
                    foreach (var eachFilePath in filePathList)
                    {
                        Attachment data = new Attachment(eachFilePath, MediaTypeNames.Application.Octet);
                        //using (Attachment data = new Attachment(eachFilePath, MediaTypeNames.Application.Octet))
                        //{
                        // Add time stamp information for the file.
                        ContentDisposition disposition = data.ContentDisposition;
                            disposition.CreationDate = File.GetCreationTime(eachFilePath);
                            disposition.ModificationDate = File.GetLastWriteTime(eachFilePath);
                            disposition.ReadDate = File.GetLastAccessTime(eachFilePath);
                            message.Attachments.Add(data);
                        //}
                    }
                    //message.Attachments.Add(data);

                    string mailbody = "A new Field Report was submitted for your review.";
                    message.Subject = "New Field Assurance report notification";
                    message.Body = mailbody;
                    message.CC.Add(new MailAddress(ccRequestor));//= ccRequestor;
                    message.BodyEncoding = Encoding.UTF8;
                    message.IsBodyHtml = true;
                    ////SmtpClient client = new SmtpClient("smtp.office365.com", 587); //Gmail smtp       
                    SmtpClient client = new SmtpClient(smtpServer, 587); //Gmail smtp       
                    NetworkCredential basicCredential1 = new
                    NetworkCredential(emailAdmin, emailAdminPassword);
                    //System.Net.NetworkCredential("bachochin.steve@cryolife.com", "mypassword");
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = basicCredential1;

                    client.Send(message);
                    
                }
                //PURGE TEMP FILES
                var tmpPath = HttpContext.Current.Server.MapPath("~/PDFFileLibrary");
                foreach (var file in Directory.GetFiles(tmpPath.ToString()))
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("Form processing error " + ex.ToString());
                return Ok("Form processing error..." + ex.ToString());
                //throw ex;
                // return NotFound();
            }
            
            return Ok("PDF file was successfully created and notification was sent");

            /////////////////////////////END PDF CREATION AND EMAIL NOTIFICATION
            
        }

        // DELETE: api/Fields/5
        [ResponseType(typeof(Field))]
        public async Task<IHttpActionResult> DeleteField(int id)
        {
            Field field = await db.Fields.FindAsync(id);
            if (field == null)
            {
                return NotFound();
            }

            db.Fields.Remove(field);
            await db.SaveChangesAsync();

            return Ok(field);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool FieldExists(int id)
        {
            return db.Fields.Count(e => e.fldid == id) > 0;
        }
        public string convertToDateOnly(String strDt)
        {
            DateTime dt = DateTime.Parse(strDt);
            return dt.ToShortDateString();
        }

        public string checkBox(string yesno)
        {
            return (yesno == "yes" ? "[X]" : "[&nbsp;]");
        }

        public string radioButton(string yesno, FieldLang langRecord)
        {
            if (yesno == "yes") {
                return "[X]&nbsp;" + langRecord.ReturnSampleY + "<br />[&nbsp;]&nbsp;" + langRecord.ReturnSampleN;
            }
            else
            {
                return "[&nbsp;]&nbsp;" + langRecord.ReturnSampleY + "<br />[X]&nbsp;" + langRecord.ReturnSampleN;
            };
        }
        /**Just sends back HTML Text for the PDF**/

        public string createHTMLTextForPDF(Field fieldRecord, string langCode)
        {

            //RECORD HAS BEEN CREATED IN BACKEND SQL
            var propertyInfo = typeof(FieldLang).GetProperty("Lang");
            var result = (
                                from record in db.FieldLangs.AsEnumerable().Where(a =>
                    propertyInfo.GetValue(a, null).ToString().ToLower().Contains(langCode.ToLower())
                )
                                select record

            ).AsQueryable();
            //ONLY RETURN THE FIRST VALUE IN THE RESULT LIST (IN CASE THERE HAPPENS TO BE TWO OF THE SAME RECORD
            var langRecord = result.First();
            //return Ok("Field Record ID " + fieldRecord.fldid);
            // Create a 'WebRequest' object with the specified url.  <IHttpActionResult>
            // WebRequest webStreamReq = WebRequest.Create("http://uskenappdev01:8010/api/Fields/" + pid);

            //1. DOCUMENT CSS
            var logoImgPath = HttpContext.Current.Server.MapPath("~/Assets/CRYLogo.jpg");
            String strHTMLPg = @"<html>
            <head>
            <META HTTP-EQUIV='content-type' CONTENT='text/html; charset=utf-8'>
            <title>Field Assurance</title>
            <style>
             * 	{
	                font-size:8pt;
	                font-family:""Times New Roman"", Times, serif;
	            }
                .frmcell{
                    border-style: solid;
                    padding:5.4pt;
                }
                .frmcelllbl{
                    border-style: solid;
                    padding:5.4pt;
                    background:#E0E0E0;
                }
                .frmcellhdr{
                    border-style: none;
                    border-bottom-style: solid;
                    padding:5.4pt;
                }
            </style>
            ";
            strHTMLPg += @"</head><body>
            <div>";
            strHTMLPg += @"<table border=1 cellspacing=0 cellpadding=0
             style='border-collapse:collapse; border:none'>";
            strHTMLPg += @"<tr style='height:31.95pt'>
              <td colspan=4 rowspan=2 style='border:none;
              border-bottom:solid windowtext 1.0pt;padding:0in 5.4pt 0in 5.4pt;height:31.95pt'><img src='" + logoImgPath + @"' alt='Logo' >
              </td>
              <td colspan=4 style='padding:0in 0in 0in 0in;
              height:31.95pt;border:none;'>
              </td>
              <td colspan=4 style='padding:0in 5.4pt 0in 5.4pt;
              height:31.95pt;border:none;'>
              </td>
             </tr>";
            strHTMLPg += @"<tr style='height:31.9pt'>
              <td colspan=4 class='frmcellhdr' style='height:31.9pt'>
              <p align=center style='text-align:center'>" + langRecord.Confidential + @"</p>
              </td>
              <td colspan=4 class='frmcellhdr' style='height:31.9pt'>
              <p align=right style='text-align:right'>" + langRecord.SOP + @"</p>
              </td>
             </tr>";
            strHTMLPg += @"<tr style='height:.2in'>
              <td colspan=8 width='1000' class='frmcell'>
              <p align=center style='text-align:center'>" + langRecord.TitleHdr + @"</p>
              </td>
              <td colspan=2 class='frmcell' class='frmcell'>
              <p align=center style='text-align:center'>" + langRecord.DocumentNumber + @"</p>
              </td>
              <td colspan=2 class='frmcell'>
              <p align=center style='text-align:center'>" + langRecord.EffectiveDate + @"</p>
              </td>
             </tr>
             <tr style='height:.3in'>
              <td colspan=8 width='1000' class='frmcell'>
              <p align=center style='text-align:center'>" + langRecord.Title + @"</p>
              </td>
              <td colspan=2 class='frmcell' style='text-align:center'>FA0002A.001
              </td>
              <td colspan=2 class='frmcell' style='padding:0in 0in 0in 0in;height:
              .3in;text-align:center'>01/02/2015
              </td>
             </tr>
             <tr style='height:.2in'>
              <td colspan=6 style='border:none;border-bottom:solid windowtext 1.0pt;
              ;padding:0in 5.4pt 0in 5.4pt;
              height:.2in'>&nbsp;
              </td>
              <td colspan=6 style='border:none;border-bottom:solid windowtext 1.0pt;
              padding:0in 5.4pt 0in 5.4pt;
              height:.2in'>&nbsp;
              </td>
             </tr>";

            strHTMLPg += @"
             <tr style='height:63.85pt'>
              <td colspan=8 width='1000' class='frmcell' style='height:63.85pt'>
              <p style='text-align:center;font-size:22px;'>" + langRecord.Title + @"</p>
              <p align=center style='text-align:center'>" + langRecord.SubTitle + @"</p>
              </td>
              <td colspan=4 class='frmcell' style='text-align: center;'>
              <p>" + langRecord.Instructions + @"</p>
              </td>
             </tr>
             <tr>
              <td colspan=12 valign=middle class='frmcell'>
              <p style='font-weight:600;'>" + langRecord.SectionA + "</p></td></tr>";

            //3. BEGIN FORM FIELDS
            strHTMLPg += @"<tr>
              <td colspan=3 valign=middle class='frmcelllbl'>
              <p>" + langRecord.CName + @"</p>
              </td>
              <td colspan=9 valign=middle class='frmcell'>" + fieldRecord.CName + @"
              </td>
             </tr>";
             strHTMLPg += @"<tr>
              <td colspan=3 valign=middle class='frmcelllbl'>
              <p>" + langRecord.FacName + @"</p>
              </td>
              <td colspan=3 width='350' valign=middle class='frmcell'>" + fieldRecord.FacName;
            strHTMLPg += @"</td>
              <td colspan=3 rowspan=2 valign=middle class='frmcelllbl'>
              <p>" + langRecord.FacAddress + @"</span></p>
              </td>
              <td colspan=3 width='350' class='frmcell' rowspan=2 valign=top >" + fieldRecord.FacAddress + @"</td></tr>";
            strHTMLPg += @"<tr style='height:12.2pt'>
              <td colspan=3 valign=middle class='frmcelllbl'>
              <p>" + langRecord.FacPhone + @"</p>
              </td>
              <td colspan=3 width='350' valign=middle class='frmcell'>" + fieldRecord.FacPhone + @"</td></tr>";

            strHTMLPg += @"<tr style='height:12.1pt'>
              <td colspan=3 valign=middle class='frmcelllbl'>
              <p>" + langRecord.SurgeonName + @"</p>
              </td>
              <td colspan=3 width='350' valign=middle class='frmcell'>" + fieldRecord.SurgeonName;
            //END SURGEON NAME
            strHTMLPg += @"</td>
              <td colspan=3 valign=middle class='frmcelllbl'>
              <p>" + langRecord.SurgeonEmail + @"</p>
              </td>
              <td colspan=3 width='350' valign=middle class='frmcell'>" + fieldRecord.SurgeonEmail;
            //END SURGEON EMAIL
            strHTMLPg += @"</td>
             </tr>
             <tr style='height:12.1pt'>
              <td colspan=3 valign=middle class='frmcelllbl'>
              <p>" + langRecord.CryolifeRep + @"</p>
              </td>
              <td colspan=3 width='350' valign=middle class='frmcell'>" + fieldRecord.CryolifeRep;
            //END CRYOLIFE REP
            strHTMLPg += @"</td>
              <td colspan=3 valign=middle class='frmcelllbl'>
              <p>" + langRecord.RegionMgr + @"</p>
              </td>
              <td colspan=3 width='350' valign=middle class='frmcell'>" + fieldRecord.RegionMgr;
            //END REGIONAL MGR
            strHTMLPg += @"</td>
             </tr>
             <tr style='height:44.5pt'>
              <td colspan=3 class='frmcelllbl'>
              <p>" + langRecord.Letter + @"</p>
              </td>
              <td colspan=3 width='350' class='frmcell'>
              <p style='margin-right:-5.4pt'>" + checkBox(fieldRecord.LetterAck) + "&nbsp;" + langRecord.LetterAck + @"<br />";
            strHTMLPg += checkBox(fieldRecord.LetterFinal) + "&nbsp;" + langRecord.LetterFinal + @"<br />";
            strHTMLPg += checkBox(fieldRecord.LetterNone) + "&nbsp;" + langRecord.LetterNone + @"</p>";
            strHTMLPg += @"</td>
              <td colspan=3 valign=middle class='frmcelllbl'>
              <p>" + langRecord.LetterSendTo + @"</p>
              </td>
              <td colspan=3 width='350' valign=middle class='frmcell' >" + fieldRecord.LetterSendTo;

            strHTMLPg += @"</td>
             </tr>
             <tr>
              <td colspan=12 valign=middle class='frmcell' >
              <p style='font-weight:600;'>" + langRecord.SectionB + @"</p>
              </td>
             </tr>
             <tr style='height:27.85pt'>
              <td colspan=3 class='frmcelllbl'>
              <p>" + langRecord.ReturnSample + @"</p>
              </td>
              <td colspan=3 width='350' class='frmcell'>" + radioButton(fieldRecord.ReturnSample, langRecord);
            strHTMLPg += @"</td>
              <td colspan=3 class='frmcelllbl'>
              <p>" + langRecord.DateIncident + @"</p>
              </td>
              <td colspan=3 width='350' valign=middle class='frmcell' >" + convertToDateOnly(fieldRecord.DateIncident.ToString());
            strHTMLPg += @"</td>
             </tr>
             <tr>
              <td colspan=3 valign=middle class='frmcelllbl'><p>" + langRecord.RMANumber + @"</p>
              </td>
              <td colspan=3 width='350' valign=middle class='frmcell' >" + fieldRecord.RMANumber;
            strHTMLPg += @"</td>
              <td colspan=3 valign=middle class='frmcelllbl'>
              <p>" + langRecord.DateReported + @"</p>
              </td>
              <td colspan=3 width='350' class='frmcell' valign=middle >" + convertToDateOnly(fieldRecord.DateReported.ToString());

            strHTMLPg += @"</td>
             </tr>
             <tr style='height:33.8pt'>
              <td colspan=3 valign=middle class='frmcelllbl'>
              <p>" + langRecord.Product + @"</p>
              </td>
              <td colspan=3 width='350' valign=middle class='frmcell'>" + fieldRecord.Product;

            strHTMLPg += @"</td>
              <td colspan=3 rowspan=4 class='frmcelllbl' valign=top >
              <p>" + langRecord.Outcome + @"</p>
              </td>
              <td colspan=3 width='350' rowspan=4  class='frmcell' valign=top >" + fieldRecord.Outcome;
            strHTMLPg += @"</td>
             </tr>
             <tr style='height:28.75pt'>
              <td colspan=3 valign=middle class='frmcelllbl'>
              <p>" + langRecord.SerialLotNumber + @"</p>
              </td>
              <td colspan=3 width='350' valign=middle class='frmcell'" + fieldRecord.SerialLotNumber;
            strHTMLPg += @"</td>
             </tr>
             <tr style='height:25.15pt'>
              <td colspan=3 valign=middle class='frmcelllbl'>
              <p>" + langRecord.UDI + @"</p>
              </td>
              <td colspan=3 width='350' class='frmcell' valign=middle >" + fieldRecord.UDI;
            strHTMLPg += @"</td>
             </tr>
             <tr style='height:25.15pt'>
              <td colspan=3 valign=middle class='frmcelllbl' >
              <p>" + langRecord.Implanted + @"</p>
              </td>
              <td colspan=3 width='350' class='frmcell' valign=middle>" + radioButton(fieldRecord.Implanted, langRecord);
            strHTMLPg += @"</td>
             </tr>
             <tr style='height:91.3pt'>
              <td colspan=3 class='frmcelllbl' valign=top >
            <p>" + langRecord.Description + @"</p>
              </td>
              <td colspan=9 valign=top class='frmcell'><p>" + fieldRecord.Description + "</p>";
            strHTMLPg += @"</td>
             </tr>
             <tr style='height:22.9pt'>
              <td colspan=3 valign=middle class='frmcelllbl' >
              <p>" + langRecord.SubmittedBy + @"</p>
              </td>
              <td colspan=4 valign=middle class='frmcell'>" + fieldRecord.SubmittedBy + @"</td>
              <td colspan=3 width='350' valign=middle class='frmcelllbl'>
              <p>" + langRecord.DateSubmitted + @"</p>
              </td>
              <td colspan=2 class='frmcell' valign=middle>" + convertToDateOnly(fieldRecord.Created.ToString()) + @"</td>
             </tr>";
            strHTMLPg += @"</table>
            </div>
            </body>
            </html>";
            return strHTMLPg;
        }
    }
}