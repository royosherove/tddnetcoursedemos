namespace BlogEngine.Core.Web.HttpModules
{
    using System;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.IO;
    using BlogEngine.Core.Web;

    /// <summary>
    /// Handles pretty URL's and redirects them to the permalinks.
    /// </summary>
    public class UrlRewrite : IHttpModule
    {
        #region Implemented Interfaces

        #region IHttpModule

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        {
            // Nothing to dispose
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application</param>
        public void Init(HttpApplication context)
        {
            context.BeginRequest += ContextBeginRequest;
        }

        #endregion

        #endregion

        /// <summary>
        /// Handles the BeginRequest event of the context control.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        private static void ContextBeginRequest(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;
            var path = context.Request.Path.ToUpperInvariant();
            var url = UrlRules.GetUrlWithQueryString(context).ToUpperInvariant();

            path = path.Replace(".ASPX.CS", string.Empty);
            url = url.Replace(".ASPX.CS", string.Empty);

            // to prevent XSS
            url = HttpUtility.HtmlEncode(url);

            Blog blogInstance = Blog.CurrentInstance;

            // bundled scripts and styles are in the ~/scripts and ~/styles
            // redirect path from ~/child/scripts/js to ~/scripts/js etc.
            if (!blogInstance.IsPrimary)
            {
                if (url.Contains("/SCRIPTS/") || url.Contains("/STYLES/"))
                {
                    var npath = url.Replace(Blog.CurrentInstance.RelativeWebRoot.ToUpper(), "/");
                    context.RewritePath(npath);
                    return;
                }
            }

            if (Utils.IsCurrentRequestForHomepage)
            {
                var front = Page.GetFrontPage();
                if (front != null)
                {
                    url = front.RelativeLink.ToUpperInvariant();
                }
            }

            Rewrite(context, blogInstance, url, path);
        }

        private static void Rewrite(HttpContext context, Blog blogInstance, string url, string path)
        {
            var urlContainsFileExtension = BlogSettings.Instance.RemoveExtensionsFromUrls ? true : 
                url.IndexOf(BlogConfig.FileExtension, StringComparison.OrdinalIgnoreCase) != -1;

            // Utils.Log(string.Format("Rewriting :: {0} :: {1}", url, path));

            if (url.Contains("/FILES/") && url.Contains(".AXDX"))
            {
                UrlRules.RewriteFilePath(context, url);
            }
            if (url.Contains("/IMAGES/") && url.Contains(".JPGX"))
            {
                UrlRules.RewriteImagePath(context, url);
            }
            if (url.Contains("/POST/"))
            {
                UrlRules.RewritePost(context, url);
            }
            else if (urlContainsFileExtension && url.Contains("/CATEGORY/"))
            {
                UrlRules.RewriteCategory(context, url);
            }
            else if (urlContainsFileExtension && url.Contains("/TAG/"))
            {
                UrlRules.RewriteTag(context, url);
            }
            else if (urlContainsFileExtension && url.Contains("/PAGE/"))
            {
                UrlRules.RewritePage(context, url);
            }
            else if (urlContainsFileExtension && url.Contains("/CALENDAR/"))
            {
                UrlRules.RewriteCalendar(context, url);
            }
            else if (urlContainsFileExtension && UrlRules.DefaultPageRequested(context))
            {
                UrlRules.RewriteDefault(context);
            }
            else if (urlContainsFileExtension && url.Contains("/AUTHOR/"))
            {
                UrlRules.RewriteAuthor(context, url);
            }
            else if (urlContainsFileExtension && (path.Contains("/BLOG.ASPX") || path.EndsWith("/BLOG")))
            {
                UrlRules.RewriteBlog(context, url);
            }
            else
            {
                // Utils.Log(string.Format("Rewriting ELSE :: {0} :: {1}", url, path));

                // If this is blog instance that is in a virtual sub-folder, we will
                // need to rewrite the path for URL to a physical file.  This includes
                // requests such as the homepage (default.aspx), contact.aspx, archive.aspx,
                // any of the admin pages, etc, etc.
                if (blogInstance.IsSubfolderOfApplicationWebRoot &&
                    VirtualPathUtility.AppendTrailingSlash(path).IndexOf(blogInstance.RelativeWebRoot, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    bool skipRewrite = false;
                    string rewriteQs = string.Empty;
                    string rewriteUrl = UrlRules.GetUrlWithQueryString(context);

                    int qsStart = rewriteUrl.IndexOf("?");
                    if (qsStart != -1)  // remove querystring.
                    {
                        rewriteQs = rewriteUrl.Substring(qsStart);
                        rewriteUrl = rewriteUrl.Substring(0, qsStart);
                    }

                    // Want to see if a specific page/file is being requested (something with a . (dot) in it).
                    // Because Utils.ApplicationRelativeWebRoot may contain a . (dot) in it, pathAfterAppWebRoot
                    // tells us if the actual path (after the AppWebRoot) contains a dot.
                    string pathAfterAppWebRoot = rewriteUrl.Substring(Utils.ApplicationRelativeWebRoot.Length);

                    if (!pathAfterAppWebRoot.Contains("."))
                    {
                        if (!rewriteUrl.EndsWith("/"))
                            rewriteUrl += "/";

                        rewriteUrl += "default.aspx";
                    }
                    else
                    {
                        if (Path.GetExtension(pathAfterAppWebRoot).ToUpperInvariant() == ".AXD")
                            skipRewrite = true;
                    }

                    if (!skipRewrite)
                    {
                        // remove the subfolder portion.  so /subfolder/ becomes /.
                        rewriteUrl = new Regex(Regex.Escape(blogInstance.RelativeWebRoot), RegexOptions.IgnoreCase).Replace(rewriteUrl, Utils.ApplicationRelativeWebRoot);

                        context.RewritePath(rewriteUrl + rewriteQs, false);
                    }

                    return;
                }
            }
        }
    }
}