using System;
using System.ComponentModel;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Xml.Serialization;
using Microsoft.Office.Server.Search.WebControls;
using Microsoft.SharePoint.Security;
using OriginalResultScriptWebPart = Microsoft.Office.Server.Search.WebControls.ResultScriptWebPart;
using OriginalScriptApplicationManager = Microsoft.Office.Server.Search.WebControls.ScriptApplicationManager;

namespace SPFSearchFix
{
    [ToolboxItemAttribute(false)]
    [ScriptDescriptorComponentType("Srch.Result")]
    [System.Runtime.InteropServices.ComVisible(false)]
    [XmlRoot(Namespace = "urn:schemas-microsoft-com:ResultScriptWebPart")]
    [SharePointPermission(System.Security.Permissions.SecurityAction.LinkDemand, ObjectModel = true)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = System.Web.AspNetHostingPermissionLevel.Minimal)]
    [SharePointPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, ObjectModel = true)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = System.Web.AspNetHostingPermissionLevel.Minimal)]
    public class ResultScriptWebPart : OriginalResultScriptWebPart
    {
        protected override void OnInit(EventArgs e)
        {
            ScriptApplicationManager.GetCurrent(this.Page);
            base.ShowMissingFeatureMessageIfNeeded();
            if (base.AppManager != null && !this.GetPrivatePropertyValue<bool>("SkipUserPreferenceFetching"))
            {
                typeof(OriginalScriptApplicationManager).GetMethod("FetchServiceAppSettings", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(base.AppManager, null);
            }

            typeof(ScriptWebPart).GetMethod("OnInit", BindingFlags.NonPublic | BindingFlags.Instance).InvokeNotOverride(this, e);
        }

        protected override void CreateChildControls()
        {
            DataProviderScriptWebPart dp = this.GetPrivatePropertyValue<DataProviderScriptWebPart>("DataProvider");

            if (dp != null)
            {
                this.Controls.Add(dp);
            }
            if (this.GetPrivateFieldValue<bool>("RenderOnServer") && !base.IsSharePointCrawler())
            {
                this.Controls.Add(this.GetPrivatePropertyValue<SearchServerRenderer>("ServerRenderer"));
            }

            typeof(DisplayScriptWebPart).GetMethod("CreateChildControls", BindingFlags.NonPublic | BindingFlags.Instance).InvokeNotOverride(this, null);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (!base.IsSharePointCrawler())
            {
                if (string.IsNullOrEmpty(base.RenderTemplateId))
                {
                    base.RenderTemplateId = "~sitecollection/_catalogs/masterpage/Display Templates/Search/Control_SearchResults.js";
                }

                typeof(DisplayScriptWebPart).GetMethod("OnLoad", BindingFlags.Instance | BindingFlags.NonPublic).InvokeNotOverride(this, e);
                this.EnsureChildControls();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.GenerateDataProviderMessage();
            if (!this.GetPrivateFieldValue<bool>("RenderOnServer") && !base.IsSharePointCrawler())
            {
                this.RegisterTemplateScript(this.ItemBodyTemplateId);
                this.RegisterTemplateScript(this.ItemTemplateId);
                this.RegisterTemplateScript(this.GroupTemplateId);
                if (this.PreloadedItemTemplateIds != null)
                {
                    string[] array = this.PreloadedItemTemplateIds;
                    for (int i = 0; i < array.Length; i++)
                    {
                        string text = array[i];
                        if (!string.IsNullOrEmpty(text))
                        {
                            this.RegisterTemplateScript(text);
                        }
                    }
                }
            }

            typeof(DisplayScriptWebPart).GetMethod("OnPreRender", BindingFlags.Instance | BindingFlags.NonPublic).InvokeNotOverride(this, e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (this.GetPrivateFieldValue<bool>("RenderOnServer"))
            {
                this.RenderChildren(writer);
            }

            typeof(DisplayScriptWebPart).GetMethod("Render", BindingFlags.Instance | BindingFlags.NonPublic).InvokeNotOverride(this, writer);
        }

        protected override void RenderWebPart(HtmlTextWriter output)
        {
            if (!this.GetPrivateFieldValue<bool>("RenderOnServer") && !base.IsSharePointCrawler())
            {
                typeof(DisplayScriptWebPart).GetMethod("RenderWebPart", BindingFlags.Instance | BindingFlags.NonPublic).InvokeNotOverride(this, output);
            }
        }
    }
}
