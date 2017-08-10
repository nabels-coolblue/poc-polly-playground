using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class FaultsController : Controller
    {
        [HttpGet]
        [Route("transient")]
        public string Transient()
        {
            var random = new Random().Next(0, 2);
            bool iShouldCompleteSuccessfully = random == 1;
            
            if (iShouldCompleteSuccessfully)
            {
                return "success";
            }
            else
            {
                throw new Exception();
            }
        }

        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
