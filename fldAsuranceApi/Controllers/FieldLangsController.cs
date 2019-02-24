using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using fldAsuranceApi.Models;

namespace fldAsuranceApi.Controllers
{
    public class FieldLangsController : ApiController
    {
        private DBEntities db = new DBEntities();
 
        // GET: api/FieldLangs
        public IQueryable<FieldLang> GetFieldLangs()
        {
            return db.FieldLangs;
        }

        // GET: api/FieldLangs/5
        [ResponseType(typeof(FieldLang))]
        public async Task<IHttpActionResult> GetFieldLang(int id)
        {
            FieldLang fieldLang = await db.FieldLangs.FindAsync(id);
            if (fieldLang == null)
            {
                return NotFound();
            }

            return Ok(fieldLang);
        }

        // PUT: api/FieldLangs/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutFieldLang(int id, FieldLang fieldLang)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != fieldLang.langid)
            {
                return BadRequest();
            }

            db.Entry(fieldLang).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FieldLangExists(id))
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

        // POST: api/FieldLangs
        [ResponseType(typeof(FieldLang))]
        public async Task<IHttpActionResult> PostFieldLang(FieldLang fieldLang)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.FieldLangs.Add(fieldLang);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = fieldLang.langid }, fieldLang);
        }

        // DELETE: api/FieldLangs/5
        [ResponseType(typeof(FieldLang))]
        public async Task<IHttpActionResult> DeleteFieldLang(int id)
        {
            FieldLang fieldLang = await db.FieldLangs.FindAsync(id);
            if (fieldLang == null)
            {
                return NotFound();
            }

            db.FieldLangs.Remove(fieldLang);
            await db.SaveChangesAsync();

            return Ok(fieldLang);
        }

        /**GET LANGUAGE BY LANG CODE     * **/

        [Route("api/FieldLangByCode/{langCode}")]
        [HttpGet, HttpPost]
        public IHttpActionResult GetFieldLangByCode(string langCode)
        {

            //string role = "en";
            var propertyInfo = typeof(FieldLang).GetProperty("Lang");
            var result = (
                                from record in db.FieldLangs.AsEnumerable().Where(a =>
                    propertyInfo.GetValue(a, null).ToString().ToLower().Contains(langCode.ToLower())
                )
                                select record

            ).AsQueryable();
            //ONLY RETURN THE FIRST VALUE IN THE RESULT LIST (IN CASE THERE HAPPENS TO BE TWO OF THE SAME RECORD
            return Ok(result.First());
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool FieldLangExists(int id)
        {
            return db.FieldLangs.Count(e => e.langid == id) > 0;
        }
    }
}