using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;

public class BSHelper
{
    public static int GetPostCountForUserID(int iUserId)
    {
        return BSUser.GetUser(iUserId).GetPosts().Count;
    }

    public static int GetCommentCount(CommentStates state)
    {
        return BSComment.GetComments(state).Count;
    }

    public static int GetCommentCount(int iUserID, CommentStates state)
    {
        return BSComment.GetCommentsByUserID(iUserID, state).Count;
    }

    public static string GetGravatar(string email)
    {
        return GetGravatar(email, 96);
    }

    public static string GetGravatar(string email, int size)
    {
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

        UTF8Encoding encoder = new UTF8Encoding();
        MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();

        byte[] hashedBytes = md5Hasher.ComputeHash(encoder.GetBytes(email));

        StringBuilder sb = new StringBuilder(hashedBytes.Length * 2);
        for (int i = 0; i < hashedBytes.Length; i++)
            sb.Append(hashedBytes[i].ToString("X2"));

        return String.Format("http://www.gravatar.com/avatar.php?gravatar_id={0}&rating={1}&size={2}&default=identicon", sb.ToString(), "G", size); ;
    }

    public static string CreateCode(String input)
    {
        string[] pattern = new string[] { "[^a-zA-Z0-9-]", "-+" };
        string[] replacements = new string[] { "-", "-" };
        input = input.Trim();
        input = input.Replace("Ç", "C");
        input = input.Replace("ç", "c");
        input = input.Replace("Ğ", "G");
        input = input.Replace("ğ", "g");
        input = input.Replace("Ü", "U");
        input = input.Replace("ü", "u");
        input = input.Replace("Ş", "S");
        input = input.Replace("ş", "s");
        input = input.Replace("İ", "I");
        input = input.Replace("ı", "i");
        input = input.Replace("Ö", "O");
        input = input.Replace("ö", "o");
        for (int i = 0; i < pattern.Length; i++)
            input = Regex.Replace(input, pattern[i], replacements[i]);
        return input;
    }

    public static string GetPermalink(string contentType, string code, string extension)
    {
        return Blogsa.Url + contentType + "/" + code + extension;
    }

    public static string GetLink(BSPost bsPost)
    {
        string strExpression = Blogsa.Settings["permaexpression"].ToString();
        Dictionary<string, string> dicExpressions = new Dictionary<string, string>();
        dicExpressions.Add("{author}", bsPost.UserName);
        dicExpressions.Add("{name}", bsPost.Code);
        dicExpressions.Add("{year}", bsPost.Date.Year.ToString());
        dicExpressions.Add("{month}", bsPost.Date.Month.ToString("00"));
        dicExpressions.Add("{day}", bsPost.Date.Day.ToString("00"));
        dicExpressions.Add("{id}", bsPost.PostID.ToString());
        Regex rex = new Regex("({(.+?)})/");
        if (bsPost.Type == PostTypes.Page)
        {
            strExpression = strExpression.Replace("{name}", bsPost.Code);
            strExpression = strExpression.Replace("{id}", bsPost.PostID.ToString());
            strExpression = rex.Replace(strExpression, "");
        }
        else
            foreach (string key in dicExpressions.Keys)
                strExpression = strExpression.Replace(key, dicExpressions[key]);

        return Blogsa.Url + strExpression;
    }

    public static string GetLink(int iPostID)
    {
        return GetLink(BSPost.GetPost(iPostID));
    }

    public static void SetPagerButtonStates(GridView gridView, GridViewRow gvPagerRow, Page page)
    {
        int itemCount = 0;
        int.TryParse(gridView.Attributes["totalItemCount"], out itemCount);
        int pageIndex = gridView.PageIndex;
        int pageCount = gridView.PageCount;
        int pageSize = gridView.PageSize;
        Control pagerControl = gvPagerRow.Controls[0].Controls[1];
        LinkButton btnFirst = (LinkButton)pagerControl.FindControl("btnFirst");
        LinkButton btnPrevious = (LinkButton)pagerControl.FindControl("btnPrevious");
        LinkButton btnNext = (LinkButton)pagerControl.FindControl("btnNext");
        LinkButton btnLast = (LinkButton)pagerControl.FindControl("btnLast");

        Literal ltPageStart = (Literal)pagerControl.FindControl("ltPageStart");
        Literal ltPageEnd = (Literal)pagerControl.FindControl("ltPageEnd");
        Literal ltTotal = (Literal)pagerControl.FindControl("ltTotal");

        if (ltPageStart != null)
        {
            ltPageStart.Text = pageIndex == 0 ? "1" : ((pageIndex * pageSize) + 1).ToString(CultureInfo.InvariantCulture);
        }

        if (ltPageEnd != null)
        {
            int pageEnd = itemCount;
            if (itemCount <= (pageIndex * pageSize))
            {
                pageEnd = (pageIndex * pageSize);
            }
            ltPageEnd.Text = pageIndex == 0 ? pageSize.ToString(CultureInfo.InvariantCulture) : pageEnd.ToString(CultureInfo.InvariantCulture);
        }

        if (ltTotal != null)
        {
            ltTotal.Text = itemCount.ToString(CultureInfo.InvariantCulture);
        }

        if (btnFirst != null)
        {
            btnFirst.Enabled = (pageIndex != 0);
        }

        if (btnPrevious != null)
        {
            btnPrevious.Enabled = (pageIndex != 0);
        }

        if (btnNext != null)
        {
            btnNext.Enabled = (pageIndex < (pageCount - 1));
        }

        if (btnLast != null)
        {
            btnLast.Enabled = (pageIndex < (pageCount - 1));
        }

        DropDownList ddlPageSelector = (DropDownList)pagerControl.FindControl("ddlPageSelector");
        ddlPageSelector.Items.Clear();
        for (int i = 1; i <= gridView.PageCount; i++)
            ddlPageSelector.Items.Add(i.ToString(CultureInfo.InvariantCulture));
        ddlPageSelector.SelectedIndex = pageIndex;
        ddlPageSelector.SelectedIndexChanged += delegate
        {
            gridView.PageIndex = ddlPageSelector.SelectedIndex;
        };
    }

    public static string GetMd5Hash(string value)
    {
        MD5 md5Hasher = MD5.Create();
        byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
        StringBuilder sB = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sB.Append(data[i].ToString("x2"));
        }
        return sB.ToString();
    }

    public static StringDictionary GetLanguageDictionary(string strFile)
    {
        StringDictionary sdLang = new StringDictionary();

        string fileName = HttpContext.Current.Server.MapPath("~/" + strFile);

        XmlDocument docLang = new XmlDocument();
        using (StreamReader sr = new StreamReader(fileName))
        {
            docLang.Load(sr);
        }

        XmlNodeList nodesWord = docLang.SelectNodes("/language/word");

        if (nodesWord != null)
            foreach (XmlNode word in nodesWord)
                if (word.Attributes != null) sdLang.Add(word.Attributes["Keyword"].Value, word.InnerText);

        return sdLang;
    }

    public static ListItemCollection LanguagesByFolder(string strFolder)
    {
        return LanguagesByFolder(strFolder, null);
    }

    public static ListItemCollection LanguagesByFolder(string strFolder, string strFilter)
    {
        strFolder = HttpContext.Current.Server.MapPath("~/" + strFolder);
        ListItemCollection licLanguages = new ListItemCollection();
        DirectoryInfo dif = new DirectoryInfo(strFolder);
        FileInfo[] fin = dif.GetFiles("*.xml");
        foreach (FileInfo item in fin)
        {
            if (!string.IsNullOrEmpty(strFilter) && !item.Name.Contains(strFilter))
                continue;
            ListItem li = new ListItem();
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(@item.FullName);
            XmlNode node = xDoc.SelectSingleNode("//language//lang");
            if (node != null)
            {
                string strLanguage = node.InnerText;
                li.Text = strLanguage;
            }
            li.Value = item.Name.Split('.').GetValue(0).ToString();
            licLanguages.Add(li);
        }
        return licLanguages;
    }

    public static string GetSystemLanguage
    {
        get
        {
            return System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag;
        }
    }

    public static string GetXmlSingleNodeValue(string xmlFile, string xmlPath)
    {
        XmlDocument xDoc = new XmlDocument();
        xDoc.Load(xmlFile);
        XmlNode node = xDoc.SelectSingleNode(xmlPath);
        if (node != null) return node.InnerText;
        else return string.Empty;
    }

    public static string GetPostState(string state, DateTime publishDate)
    {
        if (publishDate > DateTime.Now)
            return "<strong style=\"color:#E8A700\">" + Language.Admin["WaitingPost"] + "</strong>";
        else if (state == "0")
            return "<strong style=\"color:#808080\">" + Language.Admin["DraftedPost"] + "</strong>";
        else if (state == "1")
            return "<strong style=\"color:#006F9B\">" + Language.Admin["PublishedPost"] + "</strong>";
        else if (state == "2")
            return "<strong style=\"color:#FF3300\">" + Language.Admin["DeletedPost"] + "</strong>";
        else
            return "";
    }

    private static readonly Char[] Chars = "abcdefghijklmnpqrstuvwxyz1234567890".ToCharArray();

    public static string GetRandomStr(int stringSize)
    {
        String returns = "";
        Random Rnd = new Random();
        for (int i = 0; i < stringSize; i++)
        {
            returns += Chars[Rnd.Next(Chars.Length)];
        }
        return returns;
    }

    public static bool CheckEmail(string strEmail)
    {
        Regex rex = new Regex("^[\\w\\.=-]+@[\\w\\.-]+\\.[\\w]{2,3}$");
        return rex.IsMatch(strEmail);
    }

    public static bool SendMail(string strSubject, string strFrom, string strFromName, string strTo, string strToName, string strBody, bool bIsBodyHtml)
    {
        try
        {
            MailMessage message = new MailMessage();
            message.Subject = strSubject;
            message.From = new MailAddress(strFrom, strFromName);
            message.To.Add(new MailAddress(strTo, strTo));
            message.Body = strBody;
            message.IsBodyHtml = bIsBodyHtml;
            message.Priority = MailPriority.High;

            SmtpClient client = new SmtpClient(Blogsa.Settings["smtp_server"].ToString(), Convert.ToInt32(Blogsa.Settings["smtp_port"]));
            NetworkCredential smtpUserInfo = new NetworkCredential(Blogsa.Settings["smtp_user"].ToString(), Blogsa.Settings["smtp_pass"].ToString());
            client.UseDefaultCredentials = false;
            client.Credentials = smtpUserInfo;
            client.EnableSsl = Convert.ToBoolean(Blogsa.Settings["smtp_usessl"]);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            client.Send(message);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static void AddHeader(Page page, string name, string content)
    {
        HtmlMeta metaTag = new HtmlMeta();
        HtmlGenericControl genericControl = new HtmlGenericControl();
        genericControl.InnerHtml = content;
        metaTag.Name = name;
        metaTag.Content = genericControl.InnerText;
        page.Header.Controls.Add(metaTag);
    }

    public static string GetEncodedHtml(string html, bool withWhiteList)
    {
        string encodedHtml = HttpUtility.HtmlEncode(html);

        if (withWhiteList)
        {
            string[] whiteList = Blogsa.Settings["allowed_html_tags"].Value.Split(',');
            foreach (string wl in whiteList)
                encodedHtml = encodedHtml.Replace("&lt;" + wl + "&gt;", "<" + wl + ">").Replace("&lt;/" + wl + "&gt;", "</" + wl + ">");
        }

        return encodedHtml;
    }

    public static Control FindChildControl(Control sourceControl, string controlId)
    {
        if (sourceControl != null)
        {
            Control foundControl = sourceControl.FindControl(controlId);

            if (foundControl != null)
                return foundControl;

            foreach (Control c in sourceControl.Controls)
            {
                foundControl = FindChildControl(c, controlId);
                if (foundControl != null)
                    return foundControl;
            }
        }
        return null;
    }

    public static string GetPostDisplayLanguage(string languageCode)
    {
        try
        {
            CultureInfo ci = System.Globalization.CultureInfo.GetCultureInfo(languageCode);
            return ci.DisplayName;
        }
        catch (Exception ex)
        {
            //ex
        }

        return String.Empty;
    }

    public static XmlNode CreateNode(XmlDocument XDoc, string NodeName)
    {
        XmlNode XNode = XDoc.CreateNode(XmlNodeType.Element, NodeName, "");
        return XNode;
    }

    public static XmlAttribute CreateAttribute(XmlDocument XDoc, string Name, string Value)
    {
        XmlAttribute XAttr = XDoc.CreateAttribute(Name);
        XAttr.Value = Value;
        return XAttr;
    }

    public static Guid SaveWebConfig(string strConnectionString, string strProvider)
    {
        XmlDocument XDoc = new XmlDocument();
        XDoc.Load(HttpContext.Current.Server.MapPath("~/Web.config"));
        XmlNodeList nodeList = XDoc.SelectNodes("//appSettings");
        if (nodeList.Count == 1)
        {
            Guid gSetup = Guid.NewGuid();
            nodeList[0].RemoveAll();

            XmlNode XNode = CreateNode(XDoc, "add");
            XNode.Attributes.Append(CreateAttribute(XDoc, "key", "ConnectionString"));
            XNode.Attributes.Append(CreateAttribute(XDoc, "value", strConnectionString));
            nodeList[0].AppendChild(XNode);
            XNode = CreateNode(XDoc, "add");
            XNode.Attributes.Append(CreateAttribute(XDoc, "key", "Provider"));
            XNode.Attributes.Append(CreateAttribute(XDoc, "value", strProvider));
            nodeList[0].AppendChild(XNode);
            XNode = CreateNode(XDoc, "add");
            XNode.Attributes.Append(CreateAttribute(XDoc, "key", "Setup"));
            XNode.Attributes.Append(CreateAttribute(XDoc, "value", gSetup.ToString()));
            nodeList[0].AppendChild(XNode);
            XDoc.Save(HttpContext.Current.Server.MapPath("~/web.config"));
            HttpContext.Current.Session["Step"] = "LastStep";
            return gSetup;
        }
        else
        {
            return Guid.Empty;
        }
    }
}
