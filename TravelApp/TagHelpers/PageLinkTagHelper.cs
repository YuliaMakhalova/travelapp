using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TravelApp.TagHelpers
{
    public class PageLinkTagHelper : TagHelper
    {
        private IUrlHelperFactory urlHelperFactory;
        public PageLinkTagHelper(IUrlHelperFactory helperFactory)
        {
            urlHelperFactory = helperFactory;
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }
        public int PageCount { get; set; }
        public int PageCurrent { get; set; }
        public string PageAction { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);
            output.TagName = "nav";

            if (PageCurrent < 0 || PageCurrent > PageCount)
                return;

            TagBuilder tag = new TagBuilder("ul");
            tag.AddCssClass("pagination");

            var start = (PageCurrent - 2) > 0 ? PageCurrent - 2 : 1;
            var end = (PageCurrent + 2) < PageCount +1 ? PageCurrent + 2 : PageCount;

            if (PageCurrent != start)
            {
                TagBuilder li = new TagBuilder("li");
                TagBuilder link = new TagBuilder("a");
                link.Attributes["href"] = urlHelper.Action(PageAction, new { page = PageCurrent - 1 });
                TagBuilder span = new TagBuilder("span");
                span.InnerHtml.AppendHtml("&laquo;");
                link.InnerHtml.AppendHtml(span);
                li.InnerHtml.AppendHtml(link);
                tag.InnerHtml.AppendHtml(li);
            }

            for (int pageIndex = start; pageIndex <= end; pageIndex++)
            {
                TagBuilder item = CreatePageTag(pageIndex, urlHelper);
                tag.InnerHtml.AppendHtml(item);
            }

            if (PageCurrent != end)
            {
                TagBuilder li = new TagBuilder("li");
                TagBuilder link = new TagBuilder("a");
                link.Attributes["href"] = urlHelper.Action(PageAction, new { page = PageCurrent + 1 });
                TagBuilder span = new TagBuilder("span");
                span.InnerHtml.AppendHtml("&raquo;");
                link.InnerHtml.AppendHtml(span);
                li.InnerHtml.AppendHtml(link);
                tag.InnerHtml.AppendHtml(li);
            }

            output.Content.AppendHtml(tag);
        }

        TagBuilder CreatePageTag(int pageNumber, IUrlHelper urlHelper)
        {
            TagBuilder item = new TagBuilder("li");
            TagBuilder link = new TagBuilder("a");
            if (pageNumber == this.PageCurrent)
            {
                item.AddCssClass("active");
            }
            else
            {
                link.Attributes["href"] = urlHelper.Action(PageAction, new { page = pageNumber });
            }
            link.InnerHtml.Append(pageNumber.ToString());
            item.InnerHtml.AppendHtml(link);
            return item;
        }
    }
}
