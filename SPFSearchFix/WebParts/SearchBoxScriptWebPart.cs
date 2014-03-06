using System;
using System.Reflection;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Xml.Serialization;
using Microsoft.Office.Server.Search.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using OriginalScriptApplicationManager = Microsoft.Office.Server.Search.WebControls.ScriptApplicationManager;
using OriginalSearchBoxScriptWebPart = Microsoft.Office.Server.Search.WebControls.SearchBoxScriptWebPart;

namespace SPFSearchFix
{
    [ScriptDescriptorComponentType("Srch.SearchBox")]
    [System.Runtime.InteropServices.ComVisible(false)]
    [XmlRoot(Namespace = "urn:schemas-microsoft-com:SearchBoxScriptWebPart")]
    [SharePointPermission(SecurityAction.LinkDemand, ObjectModel = true)]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [SharePointPermission(SecurityAction.InheritanceDemand, ObjectModel = true)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class SearchBoxScriptWebPart : OriginalSearchBoxScriptWebPart
    {
        protected override void OnInit(EventArgs e)
        {
            ScriptApplicationManager.GetCurrent(this.Page);
            this.ShowMissingFeatureMessageIfNeeded();
            if (base.AppManager != null && !this.GetPrivatePropertyValue<bool>("SkipUserPreferenceFetching"))
            {
                typeof(OriginalScriptApplicationManager).GetMethod("FetchServiceAppSettings", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(base.AppManager, null);
            }

            typeof(ScriptWebPart).GetMethod("OnInit", BindingFlags.NonPublic | BindingFlags.Instance).InvokeNotOverride(this, e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            int maxAllowedQueries = 1;
            using (new SPMonitoredScope("SearchBoxScriptWebPart.OnPreRender", 100u, new ISPScopedPerformanceMonitor[] 		{
			    new SPSqlQueryCounter(maxAllowedQueries)
            }))
            {
                typeof(OriginalSearchBoxScriptWebPart).GetMethod("ResolveSettings", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, new object[] { });
                typeof(ScriptWebPart).GetMethod("OnPreRender", BindingFlags.Instance | BindingFlags.NonPublic).InvokeNotOverride(this, e);
            }
        }

        protected override void RenderWebPart(HtmlTextWriter output)
        {
            if (this.ServerInitialRender)
            {
                Type typeWebPartPlatform = typeof(OriginalSearchBoxScriptWebPart).Assembly.GetType("Microsoft.Office.Server.Search.WebControls.WebPartPlatform");
                object instanceWebPartPlatform = typeWebPartPlatform.GetProperty("Current", BindingFlags.Static | BindingFlags.NonPublic).GetValue(typeWebPartPlatform);
                MethodInfo wppgetLocResourceString = typeWebPartPlatform.GetMethod("GetLocResourceString", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                Type typeLocStringIdO = typeof(OriginalSearchBoxScriptWebPart).Assembly.GetType("Microsoft.Office.Server.Search.LocStringId");
                Type typeLocStringIdP = typeof(OriginalSearchBoxScriptWebPart).Assembly.GetType("Microsoft.SharePoint.Portal.WebControls.LocStringId");

                Type typeSearchCommon = typeof(OriginalSearchBoxScriptWebPart).Assembly.GetType("Microsoft.SharePoint.Portal.WebControls.SearchCommon");
                MethodInfo scGetLocResourceStringO = typeSearchCommon.GetMethod("GetLocResourceString", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeLocStringIdO }, null);
                MethodInfo scGetLocResourceStringP = typeSearchCommon.GetMethod("GetLocResourceString", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeLocStringIdP }, null);

                SPWeb web = SPContext.Current.Web;
                bool flag = this.NavigationNodes != null && this.NavigationNodes.Length > 1;
                string text = string.Empty;
                Type kv = typeof(OriginalScriptApplicationManager).Assembly.GetType("Microsoft.Office.Server.Search.WebControls.KeywordQueryReader");
                var instance = kv.GetMethod("GetInstance", new Type[] { this.Page.GetType() }).Invoke(kv, new object[] { this.Page });
                if (instance != null)
                {
                    text = (string)kv.GetProperty("Keywords", BindingFlags.Public | BindingFlags.Instance).GetValue(instance, null);
                }
                string locResourceString = this.InitialPrompt;
                if (string.IsNullOrEmpty(locResourceString))
                {
                    locResourceString = (string)wppgetLocResourceString.Invoke(instanceWebPartPlatform, new object[] { Enum.Parse(typeLocStringIdO, "SearchBox_KeywordTextBoxToolTip_Text") });
                }
                string text2 = "";
                if (string.IsNullOrWhiteSpace(text))
                {
                    text = locResourceString;
                    text2 = "ms-srch-sb-prompt ms-helperText";
                }
                string text3 = base.CsrContainerId + "_sboxdiv";
                string text4 = base.CsrContainerId + "_sbox";
                string text5 = base.CsrContainerId + "_NavButton";
                string text6 = base.CsrContainerId + "_AutoCompList";
                string text7 = base.CsrContainerId + "_NavDropdownList";
                string text8 = base.CsrContainerId + "_SearchLink";
                string scriptLiteralToEncode = "ms-srch-sbprogress";
                output.Write("<div componentid=\"{0}\" id=\"{0}\"><div id=\"SearchBox\" data-ddd=\"DDD\" name=\"Control\"><div class=\"ms-srch-sb ms-srch-sb-border\" id=\"{1}\">", HttpUtility.HtmlAttributeEncode(base.CsrContainerId), HttpUtility.HtmlAttributeEncode(text3));
                output.Write("<input type=\"text\" value=\"{0}\" maxlength=\"2048\" accessKey=\"{1}\" title=\"{2}\" id=\"{3}\" autocomplete=\"off\" autocorrect=\"off\" onkeypress=\"{4}\" onkeydown=\"{5}\" onfocus=\"{6}\" onblur=\"{7}\" class=\"ms-textSmall {8}\"/>", new object[]
			    {
				    HttpUtility.HtmlAttributeEncode((text != null) ? text : ""),
				    HttpUtility.HtmlAttributeEncode((string)scGetLocResourceStringP.Invoke(typeSearchCommon, new object[] { Enum.Parse(typeLocStringIdP, "ResultMgmt_ToggleAdvControls_AccessKey") })),
				    HttpUtility.HtmlAttributeEncode(locResourceString),
				    HttpUtility.HtmlAttributeEncode(text4),
				    string.Concat(new string[]
				    {
					    "EnsureScriptFunc('Search.ClientControls.js', 'Srch.U', function() {if (Srch.U.isEnterKey(String.fromCharCode(event.keyCode))) {$find('",
					    SPHttpUtility.EcmaScriptStringLiteralEncode(base.CsrContainerId),
					    "').search($get('",
					    SPHttpUtility.EcmaScriptStringLiteralEncode(text4),
					    "').value);return Srch.U.cancelEvent(event);}})"
				    }),
				    "EnsureScriptFunc('Search.ClientControls.js', 'Srch.U', function() {var ctl = $find('" + SPHttpUtility.EcmaScriptStringLiteralEncode(base.CsrContainerId) + "');ctl.activateDefaultQuerySuggestionBehavior();})",
				    "EnsureScriptFunc('Search.ClientControls.js', 'Srch.U', function() {var ctl = $find('" + SPHttpUtility.EcmaScriptStringLiteralEncode(base.CsrContainerId) + "');ctl.hidePrompt();ctl.setBorder(true);})",
				    "EnsureScriptFunc('Search.ClientControls.js', 'Srch.U', function() {var ctl = $find('" + SPHttpUtility.EcmaScriptStringLiteralEncode(base.CsrContainerId) + "'); if (ctl){ ctl.showPrompt(); ctl.setBorder(false);}})",
				    HttpUtility.HtmlAttributeEncode(text2)
			    });
                string text9 = null;
                if (web != null)
                {
                    text9 = SPUtility.GetThemedImageUrl(SPUtility.ConcatUrls("/", SPUtility.ConcatUrls(SPUtility.ContextLayoutsFolder, "images/searchresultui.png")), "searchresultui");
                }
                if (flag)
                {
                    output.Write("<a title=\"{0}\" id=\"{1}\" onclick=\"{2}\" href=\"{3}\" class=\"ms-srch-sb-navLink\" >", new object[]
				{
					HttpUtility.HtmlAttributeEncode((string)wppgetLocResourceString.Invoke(instanceWebPartPlatform, new object[] { Enum.Parse(typeLocStringIdO, "SearchBox_NavigationButtonToolTip_Text") })),
					HttpUtility.HtmlAttributeEncode(text5),
					"EnsureScriptFunc('Search.ClientControls.js', 'Srch.U', function() {$find('" + SPHttpUtility.EcmaScriptStringLiteralEncode(base.CsrContainerId) + "').activateDefaultNavigationBehavior();return Srch.U.cancelEvent(event);})",
					"javascript: {}"
				});
                    if (!string.IsNullOrEmpty(text9))
                    {
                        output.Write("<img src=\"{0}\" class=\"ms-srch-sb-navImg\" id=\"navImg\" alt=\"{1}\" />", HttpUtility.HtmlAttributeEncode(text9), HttpUtility.HtmlAttributeEncode((string)wppgetLocResourceString.Invoke(instanceWebPartPlatform, new object[] { Enum.Parse(typeLocStringIdO, "SearchBox_NavigationButtonToolTip_Text") })));
                    }
                    output.Write("</a>");
                }
                output.Write("<a title=\"{0}\" class=\"ms-srch-sb-searchLink\" id=\"{1}\" onclick=\"{2}\" href=\"{3}\" >", new object[]
			{
				HttpUtility.HtmlAttributeEncode((string)wppgetLocResourceString.Invoke(instanceWebPartPlatform, new object[] { Enum.Parse(typeLocStringIdO, "SearchBox_SearchButtonToolTip_Text") })),
				HttpUtility.HtmlAttributeEncode(text8),
				string.Concat(new string[]
				{
					"EnsureScriptFunc('Search.ClientControls.js', 'Srch.U', function() {$find('",
					SPHttpUtility.EcmaScriptStringLiteralEncode(base.CsrContainerId),
					"').search($get('",
					SPHttpUtility.EcmaScriptStringLiteralEncode(text4),
					"').value);})"
				}),
				"javascript: {}"
			});
                if (!string.IsNullOrEmpty(text9))
                {
                    output.Write("<img src=\"{0}\" class=\"ms-srch-sb-searchImg\" id=\"searchImg\" alt=\"{1}\" />", HttpUtility.HtmlAttributeEncode(text9), HttpUtility.HtmlAttributeEncode((string)wppgetLocResourceString.Invoke(instanceWebPartPlatform, new object[] { Enum.Parse(typeLocStringIdO, "SearchBox_SearchButtonToolTip_Text") })));
                }
                output.Write("</a>");
                if (this.ShowQuerySuggestions)
                {
                    output.Write("<div class=\"ms-qSuggest-container ms-shadow\" id=\"AutoCompContainer\">");
                    output.Write("<div id=\"{0}\"></div>", HttpUtility.HtmlAttributeEncode(text6));
                    output.Write("</div>");
                }
                if (flag)
                {
                    output.Write("<div class=\"ms-qSuggest-container ms-shadow\" id=\"NavDropdownListContainer\">");
                    output.Write("<div id=\"{0}\"></div>", HttpUtility.HtmlAttributeEncode(text7));
                    output.Write("</div>");
                }
                output.Write("</div>");
                if (this.ShowAdvancedLink && !string.IsNullOrEmpty(this.AdvancedSearchPageAddress))
                {
                    output.Write("<div class='ms-srch-sb-link'><a id='AdvancedLink' href='{0}'>{1}</a></div>", SPHttpUtility.HtmlUrlAttributeEncode(this.AdvancedSearchPageAddress), SPHttpUtility.HtmlEncode((string)wppgetLocResourceString.Invoke(instanceWebPartPlatform, new object[] { Enum.Parse(typeLocStringIdO, "SearchBox_AdvancedSearchText") })));
                }
                if (this.ShowPreferencesLink)
                {
                    string text10 = string.Empty;
                    if (web != null)
                    {
                        text10 = SPUtility.ConcatUrls(SPUtility.ContextLayoutsFolder, "EditUserPref.aspx?Source=" + SPHttpUtility.UrlKeyValueEncode(SPAlternateUrl.ContextUri.OriginalString));
                        text10 = SPUtility.ConcatUrls(web.ServerRelativeUrl, text10);
                    }
                    if (!string.IsNullOrEmpty(text10))
                    {
                        output.Write("<div class='ms-srch-sb-link'><a id='PreferencesLink' href='{0}'>{1}</a></div>", SPHttpUtility.HtmlUrlAttributeEncode(text10), SPHttpUtility.HtmlEncode((string)wppgetLocResourceString.Invoke(instanceWebPartPlatform, new object[] { Enum.Parse(typeLocStringIdO, "SearchBox_PreferencesText") })));
                    }
                }
                output.Write("</div>");
                output.Write("</div>");

                Type swpsbd = typeof(OriginalScriptApplicationManager).Assembly.GetType("Microsoft.Office.Server.Search.WebControls.ScriptWebPartScriptBehaviorDescriptor");
                var scriptWebPartScriptBehaviorDescriptor = Activator.CreateInstance(swpsbd, this.ClientControlType, base.CsrContainerId);
                typeof(ScriptObjectBuilder).GetMethod("DescribeSimpleComponent", BindingFlags.NonPublic | BindingFlags.Static).Invoke(typeof(ScriptObjectBuilder), new object[] { this, scriptWebPartScriptBehaviorDescriptor });
                string behaviorScript = (string)swpsbd.GetMethod("GetBehaviorScript", BindingFlags.Instance | BindingFlags.Public).Invoke(scriptWebPartScriptBehaviorDescriptor, new object[] { });
                this.OnAfterSerializeToClient(new ScriptWebPart.AfterSerializeToClientEventArgs(this));
                string text11 = string.Concat(new string[]
			    {
				    "\r\n                    ExecuteOrDelayUntilScriptLoaded(\r\n                        function() \r\n                        {\r\n                            if ($isNull($find('",
				    SPHttpUtility.EcmaScriptStringLiteralEncode(base.CsrContainerId),
				    "')))\r\n                            {\r\n                                var sb = ",
				    behaviorScript,
				    "\r\n                                sb.activate('",
				    SPHttpUtility.EcmaScriptStringLiteralEncode(locResourceString),
				    "', '",
				    SPHttpUtility.EcmaScriptStringLiteralEncode(text4),
				    "', '",
				    SPHttpUtility.EcmaScriptStringLiteralEncode(text3),
				    "', '",
				    SPHttpUtility.EcmaScriptStringLiteralEncode(text5),
				    "', '",
				    SPHttpUtility.EcmaScriptStringLiteralEncode(text6),
				    "', '",
				    SPHttpUtility.EcmaScriptStringLiteralEncode(text7),
				    "', '",
				    SPHttpUtility.EcmaScriptStringLiteralEncode(text8),
				    "', '",
				    SPHttpUtility.EcmaScriptStringLiteralEncode(scriptLiteralToEncode),
				    "', '",
				    SPHttpUtility.EcmaScriptStringLiteralEncode(text2),
				    "');\r\n                            }\r\n                        }, 'Search.ClientControls.js');"
			    });

                if (this.SetFocusOnPageLoad)
                {
                    Guid guid = Guid.NewGuid();
                    string text12 = text11;
                    text11 = string.Concat(new string[]
				    {
					    text12,
					    "\r\n                        function initSearchBox",
					    guid.ToString("N"),
					    "() {\r\n                            $get('",
					    SPHttpUtility.EcmaScriptStringLiteralEncode(text4),
					    "').focus();\r\n                        }\r\n                        _spBodyOnLoadFunctionNames.push('initSearchBox",
					    guid.ToString("N"),
					    "');"
				    });
                }

                if (!SPPageContentManager.IsStartupScriptRegistered(this.Page, base.GetType(), "SearchBoxScriptWebPart" + text4))
                {
                    SPPageContentManager.RegisterStartupScript(this.Page, base.GetType(), "SearchBoxScriptWebPart" + text4, text11);
                }
            }

            typeof(ScriptWebPart).GetMethod("RenderWebPart", BindingFlags.Instance | BindingFlags.NonPublic).InvokeNotOverride(this, output);
        }
    }
}