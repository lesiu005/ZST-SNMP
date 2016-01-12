using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using RestWebService.SNMPAccessLayer;

namespace RestWebService
{
    public class WebService : IHttpHandler
    {
        private String ConnString;
        private AccessLayer AccessLayer;

        public bool IsReusable
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                /*String url = Convert.ToString(context.Request.Url);  

                switch (context.Request.HttpMethod)
                {
                    case "GET":                 
                        Read(context);
                        break;
                    case "POST":
                        Create(context);
                        break;
                    case "PUT":
                        Update(context);
                        break;
                    case "DELETE":
                        Delete(context);
                        break;
                    default:
                        break;
                }*/
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                context.Response.Write(e.Message);
            }
        }

        private static void WriteResponse(string strMessage)
        {
            HttpContext.Current.Response.Write(strMessage);
        }

        /// <summary>
        /// Reads the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void Read(HttpContext context)
        {
            /*int employeeCode = Convert.ToInt16(context.Request["id"]);

            context.Response.ContentType = "text/xml";
            WriteResponse("TESTING");*/
        }

        private void Delete(HttpContext context)
        {
            throw new NotImplementedException();
        }

        private void Update(HttpContext context)
        {
            throw new NotImplementedException();
        }

        private void Create(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
