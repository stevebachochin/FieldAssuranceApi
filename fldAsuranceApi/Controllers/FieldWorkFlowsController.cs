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
    public class FieldWorkFlowsController : ApiController
    {
        private DBEntities db = new DBEntities();

        // GET: api/FieldWorkFlows
        public IQueryable<FieldWorkFlow> GetFieldWorkFlows()
        {
            return db.FieldWorkFlows;
        }

        // GET: api/FieldWorkFlows/5
        [ResponseType(typeof(FieldWorkFlow))]
        public async Task<IHttpActionResult> GetFieldWorkFlow(int id)
        {
            FieldWorkFlow fieldWorkFlow = await db.FieldWorkFlows.FindAsync(id);
            if (fieldWorkFlow == null)
            {
                return NotFound();
            }

            return Ok(fieldWorkFlow);
        }

        // PUT: api/FieldWorkFlows/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutFieldWorkFlow(int id, FieldWorkFlow fieldWorkFlow)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != fieldWorkFlow.fldwid)
            {
                return BadRequest();
            }

            db.Entry(fieldWorkFlow).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FieldWorkFlowExists(id))
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

        // POST: api/FieldWorkFlows
        [ResponseType(typeof(FieldWorkFlow))]
        public async Task<IHttpActionResult> PostFieldWorkFlow(FieldWorkFlow fieldWorkFlow)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.FieldWorkFlows.Add(fieldWorkFlow);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = fieldWorkFlow.fldwid }, fieldWorkFlow);
        }

        // DELETE: api/FieldWorkFlows/5
        [ResponseType(typeof(FieldWorkFlow))]
        public async Task<IHttpActionResult> DeleteFieldWorkFlow(int id)
        {
            FieldWorkFlow fieldWorkFlow = await db.FieldWorkFlows.FindAsync(id);
            if (fieldWorkFlow == null)
            {
                return NotFound();
            }

            db.FieldWorkFlows.Remove(fieldWorkFlow);
            await db.SaveChangesAsync();

            return Ok(fieldWorkFlow);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool FieldWorkFlowExists(int id)
        {
            return db.FieldWorkFlows.Count(e => e.fldwid == id) > 0;
        }
    }
}