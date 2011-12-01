namespace Boycott.Web {
    using System;
    using System.Web;
    using Boycott.Mapper;

    public class BoycottHttpHandler : IHttpHandler {
        #region IHttpHandler Members

        public bool IsReusable {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context) {
            var sync = new Synchronizator();
            if (context.Request.Path.Equals("/activerecord/migration")) {
                context.Response.Write("ok, I gotcha!<br/>\n");
                if (sync.DatabaseExists) {
                    sync.Initialize();
                    context.Response.Write("<table>");
                    foreach (var item in sync.Tables) {
                        context.Response.Write("<tr><td>");
                        context.Response.Write(item.Name);
                        context.Response.Write("</td><td>");
                        context.Response.Write(item.Check());
                        context.Response.Write("</td><td>");
                        if (item.Check()) {
                            context.Response.Write(item.GetDiff());
                        }
                        context.Response.Write("</td></tr>");
                    }
                    context.Response.Write("</table>");
                    context.Response.Write("<a href=\"/activerecord/migrate\">migrate</a>");
                } else {
                    context.Response.Write("Database do not exist!<br/>");
                    context.Response.Write("<a href=\"/activerecord/database_create\">create database</a>");
                }
            } else if (context.Request.Path.Equals("/activerecord/migrate")) {
                sync.Initialize();
                context.Response.Write("migrate it<br/>\n");
                foreach (var item in sync.Tables) {
                    if (item.Check()) {
                        foreach (var sql in item.GetDiff().ToSql()) {
                            context.Response.Write(sql);
                            context.Response.Write("<br/>\n");
                        }
                    }
                }
                if (sync.Sync()) {
                    context.Response.Redirect("/activerecord/migration");
                }
                context.Response.Write("Done!");
            } else if (context.Request.Path.Equals("/activerecord/database_create")) {
                try {
                    Configuration.DatabaseProvider.CreateDatabase();
                    context.Response.Redirect("/activerecord/migration");
                } catch (Exception ex) {
                    context.Response.Write(ex);
                }
            }
        }
        
        #endregion
    }
}
