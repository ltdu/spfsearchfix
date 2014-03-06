using System;
using System.Reflection;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Xml.Serialization;
using Microsoft.Office.Server.Search.WebControls;
using Microsoft.SharePoint.Security;
using OriginalRefinementScriptWebPart = Microsoft.Office.Server.Search.WebControls.RefinementScriptWebPart;
using OriginalScriptApplicationManager = Microsoft.Office.Server.Search.WebControls.ScriptApplicationManager;

namespace SPFSearchFix
{
    [ScriptDescriptorComponentType("Srch.Refinement")]
    [System.Runtime.InteropServices.ComVisible(false)]
    [XmlRoot(Namespace = "urn:schemas-microsoft-com:RefinementScriptWebPart")]
    [SharePointPermission(SecurityAction.LinkDemand, ObjectModel = true)]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [SharePointPermission(SecurityAction.InheritanceDemand, ObjectModel = true)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class RefinementScriptWebPart : OriginalRefinementScriptWebPart
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

        protected override void OnLoad(EventArgs e)
        {
            if (string.IsNullOrEmpty(base.RenderTemplateId))
            {
                base.RenderTemplateId = "~sitecollection/_catalogs/masterpage/Display Templates/Filters/Control_Refinement.js";
            }

            typeof(DisplayScriptWebPart).GetMethod("OnLoad", BindingFlags.Instance | BindingFlags.NonPublic).InvokeNotOverride(this, e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.GenerateDataProviderMessage();
            typeof(OriginalRefinementScriptWebPart).GetMethod("GenerateFacetedNavigationMessage", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this, new object[] { });
            if (!this.GetPrivateFieldValue<bool>("RenderOnServer") && !base.IsSharePointCrawler() && this.SelectedRefinementControls != null)
            {
                RefinementControl[] selectedRefinementControls = this.SelectedRefinementControls;
                for (int i = 0; i < selectedRefinementControls.Length; i++)
                {
                    RefinementControl refinementControl = selectedRefinementControls[i];
                    if (refinementControl != null)
                    {
                        this.RegisterTemplateScript(refinementControl.renderTemplateId);
                    }
                }
            }

            typeof(DisplayScriptWebPart).GetMethod("OnPreRender", BindingFlags.Instance | BindingFlags.NonPublic).InvokeNotOverride(this, e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            typeof(DisplayScriptWebPart).GetMethod("Render", BindingFlags.Instance | BindingFlags.NonPublic).InvokeNotOverride(this, writer);
        }

        protected override void RenderWebPart(HtmlTextWriter output)
        {
            typeof(DisplayScriptWebPart).GetMethod("RenderWebPart", BindingFlags.Instance | BindingFlags.NonPublic).InvokeNotOverride(this, output);
        }
    }
}
