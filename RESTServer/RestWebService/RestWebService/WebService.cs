using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RestWebService
{
    public class WebService : IHttpHandler
    {
        private String ConnString;
        public WebService()
        {

        }

        public bool IsReusable
        {
            get { throw new NotImplementedException(); }
        }

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                String url = Convert.ToString(context.Request.Url);
                ConnString = Properties.Settings.Default.ConnectionString;
                dal = new DAL.DAL(connString);
                errHandler = new ErrorHandler.ErrorHandler();

                //Handling CRUD
                switch (context.Request.HttpMethod)
                {
                    case "GET":
                        //Perform READ Operation                   
                        READ(context);
                        break;
                    case "POST":
                        //Perform CREATE Operation
                        CREATE(context);
                        break;
                    case "PUT":
                        //Perform UPDATE Operation
                        UPDATE(context);
                        break;
                    case "DELETE":
                        //Perform DELETE Operation
                        DELETE(context);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {

                errHandler.ErrorMessage = ex.Message.ToString();
                context.Response.Write(errHandler.ErrorMessage);
            }
        }
    }
}
