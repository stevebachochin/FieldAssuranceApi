using System;
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
    public class KeywordsController : ApiController
    {
        private DBEntities db = new DBEntities();
        //public async Task<IHttpActionResult> PostField(Field fieldRecord)
        // GET: api/Keywords
        [HttpPost]
        [Route("api/keywords")]
        public IHttpActionResult PostKeywords(HttpRequestMessage request)
        {
            var req = Request;
            var headers = req.Headers;         

            IEnumerable<string> headerValues = request.Headers.GetValues("App");
            var id = headerValues.FirstOrDefault();
            if(id == "Field Assurance")
            {
                return Ok(db.Keywords);
            }
 
            //return Ok(db.Keywords);
            return NotFound();
        }

        // GET: api/Keywords/5
        [HttpPost]
        [Route("api/keywords/{kid}")]
        [ResponseType(typeof(Keyword))]
        public IHttpActionResult GetKeyword(HttpRequestMessage request, int kid)
        {
            Keyword keyword = db.Keywords.Find(kid);
            if (keyword == null)
            {
                return NotFound();
            }
            
            IEnumerable<string> headerValues = request.Headers.GetValues("App");
            var id = headerValues.FirstOrDefault();
            if (id == "Field Assurance")
            {
                return Ok(keyword);
            }
           
            return NotFound();
        }

        // PUT: api/Keywords/5
        [HttpPut]
        [Route("api/keywords/{id}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutKeyword(int id, Keyword keyword)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != keyword.kid)
            {
                return BadRequest();
            }

            db.Entry(keyword).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KeywordExists(id))
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

        // POST: api/Keywords
        [ResponseType(typeof(Keyword))]
        public IHttpActionResult PostKeyword(Keyword keyword)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Keywords.Add(keyword);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (KeywordExists(keyword.kid))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = keyword.Keyword1 }, keyword);
        }

        // DELETE: api/Keywords/5
        [ResponseType(typeof(Keyword))]
        public IHttpActionResult DeleteKeyword(string id)
        {
            Keyword keyword = db.Keywords.Find(id);
            if (keyword == null)
            {
                return NotFound();
            }

            db.Keywords.Remove(keyword);
            db.SaveChanges();

            return Ok(keyword);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool KeywordExists(int id)
        {
            return db.Keywords.Count(e => e.kid == id) > 0;
        }
    }
}