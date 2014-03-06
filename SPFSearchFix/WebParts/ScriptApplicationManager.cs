using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Web;
using System.Web.UI;
using Microsoft.Office.Server.Search.Administration;
using Microsoft.Office.Server.Search.Query;
using Microsoft.Office.Server.Search.WebControls;
using Microsoft.Office.Server.Utilities;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;
using OriginalScriptApplicationManager = Microsoft.Office.Server.Search.WebControls.ScriptApplicationManager;

namespace SPFSearchFix
{
    public static class ScriptApplicationManager
    {
        public static OriginalScriptApplicationManager GetCurrent(Page page)
        {
            if (page == null)
            {
                HttpContext current = HttpContext.Current;
                if (current != null)
                {
                    OriginalScriptApplicationManager mgr = (current.Items["ScriptApplicationManager"] as OriginalScriptApplicationManager);
                    if (mgr != null)
                    {
                        return mgr;
                    }
                }
            }
            else
            {
                OriginalScriptApplicationManager mgr = (page.Items["ScriptApplicationManager"] as OriginalScriptApplicationManager);
                if (mgr != null)
                {
                    return mgr;
                }
            }

            OriginalScriptApplicationManager that = FormatterServices.GetUninitializedObject(typeof(OriginalScriptApplicationManager)) as OriginalScriptApplicationManager;
            Type tm = typeof(OriginalScriptApplicationManager).Assembly.GetType("Microsoft.Office.Server.Search.Query.TraceManager");
            typeof(OriginalScriptApplicationManager).GetField("TraceLog", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(that, Activator.CreateInstance(tm));
            that.SetPrivateFieldValue<int>("defaultUserPreferenceLanguage", -1);
            that.SetPrivateFieldValue<int>("browserLangage", -1);
            that.SetPrivateFieldValue<Dictionary<string, object>>("states", new Dictionary<string, object>(System.StringComparer.Ordinal));
            that.SetPrivateFieldValue<Dictionary<string, QueryGroup>>("queryGroups", new System.Collections.Generic.Dictionary<string, QueryGroup>(System.StringComparer.Ordinal));

            using (new SPMonitoredScope("ScriptApplicationManager"))
            {
                Type saut = typeof(SearchServiceApplication).Assembly.GetType("Microsoft.Office.Server.Search.Administration.SearchAdminUtils");
                Type uut = typeof(TimerJobUtility).Assembly.GetType("Microsoft.Office.Server.Utilities.UrlUtility");

                if (HttpContext.Current != null)
                {
                    HttpRequest request = HttpContext.Current.Request;
                    if (request != null)
                    {
                        string[] userLanguages = request.UserLanguages;
                        if (userLanguages != null && userLanguages.Length > 0)
                        {
                            CultureInfo cultureInfo = null;
                            try
                            {
                                cultureInfo = new CultureInfo(userLanguages[0]);
                            }
                            catch (Exception)
                            {

                            }
                            if (cultureInfo != null)
                            {
                                that.States["browserLanguage"] = cultureInfo.LCID;
                                that.SetPrivateFieldValue<int>("browserLangage", cultureInfo.LCID);
                            }
                        }
                        if (request.QueryString["st"] != null)
                        {
                            that.States["shipTrace"] = true;
                        }
                    }
                    if (SPContext.Current != null)
                    {
                        SPWeb web = SPContext.Current.Web;
                        if (web != null)
                        {
                            that.States["webUILanguageName"] = (string)typeof(OriginalScriptApplicationManager).GetMethod("GetWebUILanguage", BindingFlags.NonPublic | BindingFlags.Static).Invoke(typeof(OriginalScriptApplicationManager), new object[] { web });
                            that.States["webDefaultLanguageName"] = new CultureInfo((int)web.Language).Name;
                            if (web.IsAppWeb)
                            {
                                that.States["contextUrl"] = web.NonHostHeaderUrl;
                            }
                            else
                            {
                                that.States["contextUrl"] = web.Url;
                            }

                            that.States["contextTitle"] = web.Title;
                            List<LanguagePreference> list = new List<LanguagePreference>();
                            int[] queryLanguages = new int[] { 1025, 1093, 1026, 1027, 2052, 1028, 1050, 1029, 1030, 1043, 1033, 1035, 1036, 1031, 1032, 1095, 1037, 1081, 1038, 1039, 1057, 1040, 1041, 1099, 1042, 1062, 1063, 1086, 1100, 1102, 1044, 1045, 1046, 2070, 1094, 1048, 1049, 3098, 2074, 1051, 1060, 3082, 2058, 1053, 1097, 1098, 1054, 1055, 1058, 1056, 1066 };
                            for (int i = 0; i < queryLanguages.Length; i++)
                            {
                                int num = queryLanguages[i];
                                string queryLanguageDisplayName = (string)saut.GetMethod("GetQueryLanguageDisplayName", BindingFlags.Static | BindingFlags.NonPublic).Invoke(saut, new object[] { num });
                                if (!string.IsNullOrEmpty(queryLanguageDisplayName))
                                {
                                    list.Add(new LanguagePreference(num, queryLanguageDisplayName));
                                }
                            }
                            that.States["supportedLanguages"] = list;
                            that.States["navigationNodes"] = (NavigationNode[])typeof(NavigationNode).GetProperty("CurrentNavigationCollection", BindingFlags.NonPublic | BindingFlags.Static).GetValue(typeof(NavigationNode));
                        }
                    }
                }

                Type sct = typeof(OriginalScriptApplicationManager).Assembly.GetType("Microsoft.Office.Server.Search.WebControls.SearchCommon");
                string searchCenterUrl = (string)sct.GetMethod("GetSearchCenterUrl", BindingFlags.Static | BindingFlags.NonPublic).Invoke(sct, new object[] { });

                if (!string.IsNullOrEmpty(searchCenterUrl))
                {
                    that.States["searchCenterUrl"] = searchCenterUrl;
                }
                that.States["showAdminDetails"] = (bool)typeof(OriginalScriptApplicationManager).GetMethod("ShouldUserSeeAdminDetails", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(that, new object[] { });
                that.States["defaultPagesListName"] = SPUtility.GetLocalizedString("$Resources:osrvcore,List_Pages_UrlName;", null, checked((uint)Thread.CurrentThread.CurrentUICulture.LCID));  //PageUtility.DefaultPagesListName;
                that.States["isSPFSKU"] = (bool)saut.GetMethod("IsSPFSKU", BindingFlags.Static | BindingFlags.NonPublic).Invoke(saut, new object[] { });
                that.States["userAdvancedLanguageSettingsUrl"] = uut.GetMethod("SafeAppendQueryStringParameter", BindingFlags.NonPublic | BindingFlags.Static).Invoke(uut, new object[] { SPUtility.ConcatUrls(SPContext.Current.Web.ServerRelativeUrl, SPUtility.ConcatUrls(SPUtility.ContextLayoutsFolder, "regionalsetng.aspx?type=user&Source=" + SPHttpUtility.UrlKeyValueEncode(SPAlternateUrl.ContextUri.ToString()))), "ShowAdvLang", "1" }); // SearchCommon.GetUserAdvancedLanguageSettingsUrl();
                KeywordQueryProperties keywordQueryProperties = new KeywordQueryProperties();
                if (keywordQueryProperties != null)
                {
                    DefaultQueryProperties defaultQueryProperties = new DefaultQueryProperties();
                    defaultQueryProperties.culture = keywordQueryProperties.Culture.LCID;
                    defaultQueryProperties.uiLanguage = keywordQueryProperties.UILanguage;
                    defaultQueryProperties.summaryLength = keywordQueryProperties.SummaryLength;
                    defaultQueryProperties.desiredSnippetLength = keywordQueryProperties.DesiredSnippetLength;
                    defaultQueryProperties.enableStemming = keywordQueryProperties.EnableStemming;
                    defaultQueryProperties.enablePhonetic = keywordQueryProperties.EnablePhonetic;
                    defaultQueryProperties.enableNicknames = keywordQueryProperties.EnableNicknames;
                    defaultQueryProperties.trimDuplicates = keywordQueryProperties.TrimDuplicates;
                    defaultQueryProperties.bypassResultTypes = keywordQueryProperties.BypassResultTypes;
                    defaultQueryProperties.enableInterleaving = keywordQueryProperties.EnableInterleaving;
                    defaultQueryProperties.enableQueryRules = keywordQueryProperties.EnableQueryRules;
                    defaultQueryProperties.processBestBets = keywordQueryProperties.ProcessBestBets;
                    defaultQueryProperties.enableOrderingHitHighlightedProperty = keywordQueryProperties.EnableOrderingHitHighlightedProperty;
                    defaultQueryProperties.hitHighlightedMultivaluePropertyLimit = keywordQueryProperties.HitHighlightedMultivaluePropertyLimit;
                    defaultQueryProperties.processPersonalFavorites = keywordQueryProperties.ProcessPersonalFavorites;
                    that.States["defaultQueryProperties"] = defaultQueryProperties;
                }
            }

            if (page == null)
            {
                HttpContext current = HttpContext.Current;
                if (current != null)
                {
                    current.Items["ScriptApplicationManager"] = that;
                }
            }
            else
            {
                page.Items["ScriptApplicationManager"] = that;
                page.Load += delegate(object o, EventArgs e)
                {
                    typeof(OriginalScriptApplicationManager).GetMethod("EnsureMySiteUrl", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(that, new object[] { page });
                };

                //page.PreRenderComplete += new EventHandler(mgr.SerializeToClient);
                page.PreRenderComplete += delegate(object o, EventArgs e)
                {
                    typeof(OriginalScriptApplicationManager).GetMethod("SerializeToClient", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(that, new object[] { o, e });
                };

                //page.PreRender += new EventHandler(mgr.ConfigureWebPartThatShouldDisplayDataErrors);
                page.PreRender += delegate(object o, EventArgs e)
                {
                    typeof(OriginalScriptApplicationManager).GetMethod("ConfigureWebPartThatShouldDisplayDataErrors", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(that, new object[] { o, e });
                };

                //page.Unload += new EventHandler(mgr.ReportTraceTime);
                page.Unload += delegate(object o, EventArgs e)
                {
                    typeof(OriginalScriptApplicationManager).GetMethod("ReportTraceTime", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(that, new object[] { o, e });
                };
            }

            return that;
        }
    }
}