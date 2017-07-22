//  Copyright 2015 Stefan Negritoiu (FreeBusy). See LICENSE file for more information.

using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace RokuControlSkill.Controllers
{
    public class AlexaController : ApiController
    {
        [Route("alexa/rokucontrol")]
        [HttpPost]
        public async Task<HttpResponseMessage> SampleSession()
        {
            var speechlet = new RokuControlSpeechlet();
            return await speechlet.GetResponseAsync(Request);
        }
    }
}
