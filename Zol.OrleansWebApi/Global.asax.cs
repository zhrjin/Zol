using Zol.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Zol.OrleansWebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var lastError = Server.GetLastError();
            if (lastError != null)
            {
                string msg = "";
                var httpError = lastError as HttpException;
                if (httpError is HttpException)
                {
                    msg = "服务错误代码:" + httpError.ErrorCode + "\r\n客户机IP:" + Request.UserHostAddress + "\r\n错误地址:" + Request.Url + "\r\n异常信息:" + lastError.Message;
                }

                Logger.Error("应用程序异常," + msg, lastError);
                Server.ClearError();
            }
        }
    }
}
