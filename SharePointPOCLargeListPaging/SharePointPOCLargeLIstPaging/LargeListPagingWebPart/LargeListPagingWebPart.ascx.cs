using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace SharePointPOCLargeLIstPaging.LargeListPagingWebPart
{

    public class ListItemPropeties
    {
        public int ID { get; set; }
        public string Title { get; set; }

        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }

    [ToolboxItemAttribute(false)]
    public partial class LargeListPagingWebPart : WebPart
    {
        // Uncomment the following SecurityPermission attribute only when doing Performance Profiling on a farm solution
        // using the Instrumentation method, and then remove the SecurityPermission attribute when the code is ready
        // for production. Because the SecurityPermission attribute bypasses the security check for callers of
        // your constructor, it's not recommended for production purposes.
        // [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, UnmanagedCode = true)]
        public LargeListPagingWebPart()
        {
        }

        private string listName = "Large List";
       

        private int CurrentPage = 1;
        private string _PageInfo = "";
        private bool _EndOfList = false;

        private int TotalItemCount = 0;

        private string fields = string.Concat(
                              "<FieldRef Name='Title' />",
                               "<FieldRef Name='ID' />",
                               "<FieldRef Name='Created' />",
                                "<FieldRef Name='Modified' />"
                             );

        private string camlQuery = "";

        private uint _pageSize = 0;

        LinkButton prevLink;
        LinkButton nextLink;
        Label lblPagingInfo;


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            InitializeControl();

            prevLink = new LinkButton();
            prevLink.Text = "<<";
            prevLink.ID = "prevLink";
            prevLink.Click += (s, ev) =>
            {
                prevLink_Click();
            };

            nextLink = new LinkButton();
            nextLink.Text = ">>";
            nextLink.ID = "nextLink";
            nextLink.Click += (s, ev) =>
            {
                nextLink_Click();
            };

            lblPagingInfo = new Label();
            lblPagingInfo.CssClass = "pageInfo";

            Panel navigationPanel = new Panel();
            navigationPanel.Controls.Add(prevLink);
            navigationPanel.Controls.Add(lblPagingInfo);
            navigationPanel.Controls.Add(nextLink);
            _ContainerPanel.Controls.Add(navigationPanel);
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            _pageSize = Convert.ToUInt32(ddlPageSize.SelectedValue);

            if (!Page.IsPostBack)
            {
                camlQuery = getSoryByQuery(ddlSortBy.SelectedValue);
                resetViewState();
                loadGrid();
            }
        }

        protected void ddlSortBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            camlQuery = getSoryByQuery(ddlSortBy.SelectedValue);
            resetViewState();
            loadGrid();
        }

        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            _pageSize = Convert.ToUInt32(ddlPageSize.SelectedValue);
            camlQuery = getSoryByQuery(ddlSortBy.SelectedValue);
            resetViewState();
            loadGrid();
        }

        private void resetViewState()
        {
            ViewState.Remove("PreviousPage");
            ViewState.Remove("NextPage");
            ViewState.Remove("Page");
            ViewState.Remove("TotalItemCount");
        }

        private string getSoryByQuery(string selectedValue)
        {
            string query = string.Empty;
            switch (selectedValue)
            {
                case "CreatedAsc":
                    query = "<OrderBy><FieldRef Name='Created' /></OrderBy>";
                    break;
                case "CreatedDesc":
                    query = "<OrderBy><FieldRef Name='Created' Ascending='False' /></OrderBy>";
                    break;
                case "ModifiedAsc":
                    query = "<OrderBy><FieldRef Name='Modified' /></OrderBy>";
                    break;
                case "ModifiedDesc":
                    query = "<OrderBy><FieldRef Name='Modified' Ascending='False' /></OrderBy>";
                    break;
                case "IdAsc":
                    query = "<OrderBy><FieldRef Name='ID' /></OrderBy>";
                    break;
                case "IdDesc":
                    query = "<OrderBy><FieldRef Name='ID' Ascending='False' /></OrderBy>";
                    break;
                case "TitleAsc":
                    query = "<OrderBy><FieldRef Name='Title' /></OrderBy>";
                    break;
                case "TitleDesc":
                    query = "<OrderBy><FieldRef Name='Title' Ascending='False' /></OrderBy>";
                    break;
                default:
                    break;
            }
            return query;
        }

        private void loadGrid()
        {
            using (SPSite site = new SPSite(SPContext.Current.Web.Url))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    SPList list = web.Lists[listName];
                    SPListItemCollection items = GetListPage(list, _pageSize, CurrentPage, _PageInfo, fields, camlQuery, out _EndOfList);
                    Render(items);
                }
            }
        }

        private void Render(SPListItemCollection pageItems)
        {
            if (CurrentPage == 1 && pageItems.Count == 0)
            {
                lblErrorMessage.Text = "No itmes to display.....";
                return;
            }

            List<ListItemPropeties> lstMarketing = new List<ListItemPropeties>();

            foreach (SPListItem item in pageItems)
            {
                ListItemPropeties marketing = new ListItemPropeties();
                marketing.ID = item.ID;
                marketing.Title = Convert.ToString(item["Title"]);
                marketing.Created = Convert.ToDateTime(item["Created"]);
                marketing.Modified = Convert.ToDateTime(item["Modified"]);
                lstMarketing.Add(marketing);
            }
            grdView.DataSource = lstMarketing;
            grdView.DataBind();

            addPagingLinks(pageItems[0], pageItems[pageItems.Count - 1]);
            displayPagingInformation();
        }

        private void displayPagingInformation()
        {
            lblPagingInfo.Text = ((CurrentPage - 1) * _pageSize) + 1 + " - " + CurrentPage * _pageSize;

            lblPageStatastics.Text = ((CurrentPage - 1) * _pageSize) + 1 + " TO " + CurrentPage * _pageSize + " OF " + TotalItemCount + " Items";
        }

        private SPListItemCollection GetListPage(SPList list,
                                                 uint pageSize,
                                                 int pageIndex,
                                                 string pagingInfo,
                                                 string fields,
                                                 string queryCaml,
                                                 out bool isEndOfList)
        {

            SPQuery query = new SPQuery();

            query.RowLimit = pageSize;
            query.ViewFields = fields;
            query.Query = queryCaml;

            if (!string.IsNullOrEmpty(pagingInfo))
            {
                SPListItemCollectionPosition collectionPosition = new SPListItemCollectionPosition(pagingInfo);
                query.ListItemCollectionPosition = collectionPosition;
            }

            SPListItemCollection returnValue = list.GetItems(query);

            if (ViewState["TotalItemCount"] == null)
            {
                TotalItemCount = getListItemsCount(listName, queryCaml);
                ViewState["TotalItemCount"] = TotalItemCount;
            }
            else
                TotalItemCount = Convert.ToInt32(ViewState["TotalItemCount"]);

            isEndOfList = (((pageIndex - 1) * pageSize) + returnValue.Count) >= TotalItemCount;

            return returnValue;
        }

        private int getListItemsCount(string listName, string camlQuery)
        {
            int count = 0;
            using (SPSite site = new SPSite(SPContext.Current.Web.Url))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    string viewFields = "<FieldRef Name='ID' />";
                    SPList list = web.Lists[listName];
                    SPQuery query = new SPQuery();
                    query.Query = camlQuery;
                    query.ViewFields = viewFields;
                    query.ViewFieldsOnly = true;
                    count = list.GetItems(query).Count;
                }
            }
            return count;
        }

        private void getSortFieldInformation(SPListItem item, out string field, out string value)
        {
            string fieldName = string.Empty;
            string fieldValue = string.Empty;

            switch (ddlSortBy.SelectedValue)
            {
                case "CreatedAsc":
                case "CreatedDesc":
                    {
                        fieldName = "Created";
                        DateTime createdDate = Convert.ToDateTime(item["Created"]);
                        fieldValue = createdDate.ToString("yyyyMMdd HH:mm:ss");
                    }
                    break;
                case "ModifiedAsc":
                case "ModifiedDesc":
                    {
                        fieldName = "Modified";
                        DateTime modifiedDate = Convert.ToDateTime(item["Modified"]);
                       
                        fieldValue = modifiedDate.ToString("yyyyMMdd HH:mm:ss");
                    }
                    break;
                case "IdAsc":
                case "IdDesc":
                    {
                        fieldName = "ID";
                        fieldValue = Convert.ToString(item["ID"]);
                    }
                    break;
                case "TitleAsc":
                case "TitleDesc":
                    {
                        fieldName = "Title";
                        fieldValue = Convert.ToString(item["Title"]);
                    }
                    break;
            }
            fieldValue = SPEncode.UrlEncode(fieldValue);

            field = fieldName;
            value = fieldValue;
        }

        private string getNextPageInfo(SPListItem lastItem)
        {
            string field = string.Empty;
            string value = string.Empty;
            int lastItemId = (int)lastItem["ID"];
            string pageInfo = string.Empty;

            getSortFieldInformation(lastItem, out field, out value);

            pageInfo = string.Format("Paged=TRUE&p_{0}={1}&p_ID={2}",
                                           field,
                                           value,
                                           lastItemId
                                           );

            return pageInfo;
        }

        private string getPreviousPageInfo(SPListItem firstItem)
        {
            string field = string.Empty;
            string value = string.Empty;
            int firstItemId = (int)firstItem["ID"];
            string pageInfo = string.Empty;

            getSortFieldInformation(firstItem, out field, out value);

            pageInfo = string.Format("Paged=TRUE&PagedPrev=TRUE&p_{0}={1}&p_ID={2}",
                                              field,
                                             value,
                                             firstItemId
                                              );

            return pageInfo;
        }

        private void addPagingLinks(SPListItem firstItem, SPListItem lastItem)
        {
            string nextPage = getNextPageInfo(lastItem);

            string previousPage;

            if (CurrentPage > 2)
            {
                previousPage = getPreviousPageInfo(firstItem);
            }
            else
            {
                previousPage = "Paged=TRUE";
            }

            ViewState["PreviousPage"] = previousPage;
            ViewState["NextPage"] = nextPage;
            ViewState["Page"] = CurrentPage;

            prevLink.Visible = true;
            nextLink.Visible = true;

            if (CurrentPage > 1)
            {
                prevLink.Visible = true;
            }
            else
            {
                prevLink.Visible = false;
            }

            if (!_EndOfList)
            {
                nextLink.Visible = true;
            }
            else
            {
                nextLink.Visible = false;
            }
        }

        private void nextLink_Click()
        {

            if (ViewState["Page"] != null)
            {
                int value;
                if (int.TryParse(Convert.ToString(ViewState["Page"]), out value))
                {
                    CurrentPage = value + 1;
                    ViewState["Page"] = CurrentPage;
                }
            }

            if (ViewState["NextPage"] != null)
            {
                _PageInfo = Convert.ToString(ViewState["NextPage"]);
            }

            camlQuery = getSoryByQuery(ddlSortBy.SelectedValue);
            loadGrid();
        }

        private void prevLink_Click()
        {

            if (ViewState["Page"] != null)
            {
                int value;
                if (int.TryParse(Convert.ToString(ViewState["Page"]), out value))
                {
                    CurrentPage = value - 1;
                    ViewState["Page"] = CurrentPage;
                }
            }

            if (ViewState["PreviousPage"] != null)
            {
                _PageInfo = Convert.ToString(ViewState["PreviousPage"]);
            }

            camlQuery = getSoryByQuery(ddlSortBy.SelectedValue);
            loadGrid();
        }
    }
}
