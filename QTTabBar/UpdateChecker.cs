//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2010  Quizo, Paul Accisano
//
//    QTTabBar is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    QTTabBar is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with QTTabBar.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace QTTabBarLib {
    internal static class UpdateChecker {
        private static bool fCheckDone;
        private const int INTERVAL_CHECK_DAY = 5;
        private static string strMsgCaption = ("QTTabBar " + QTUtility2.MakeVersionString());

        public static void Check(bool fForce) {
            if(fForce) {
                string str;
                ShowMsg(CheckInternal(out str), str);
                SaveLastCheck();
            }
            else if(!fCheckDone) {
                fCheckDone = true;
                if(DayHasCome()) {
                    string str2;
                    int num2 = CheckInternal(out str2);
                    SaveLastCheck();
                    if(num2 == 2) {
                        MessageForm.Show(string.Format(QTUtility.TextResourcesDic["UpdateCheck"][0], str2), strMsgCaption, Resources_String.SiteURL, MessageBoxIcon.Asterisk, 0xea60);
                    }
                }
            }
        }

        private static int CheckInternal(out string msg) {
            HttpStatusCode statusCode;
            msg = null;
            string str;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Resources_String.SiteURL + "/files/latestversion.txt");
            request.Timeout = 0x1388;
            try {
                using(HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                    statusCode = response.StatusCode;
                    using(StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.ASCII)) {
                        str = reader.ReadToEnd();
                    }
                }
            }
            catch(WebException exception) {
                WebExceptionStatus status = exception.Status;
                if(status != WebExceptionStatus.NameResolutionFailure) {
                    if(status == WebExceptionStatus.ProtocolError) {
                        statusCode = HttpStatusCode.InternalServerError;
                        if((exception.Response != null) && (exception.Response is HttpWebResponse)) {
                            statusCode = ((HttpWebResponse)exception.Response).StatusCode;
                        }
                        return ((statusCode == HttpStatusCode.NotFound) ? -1 : -2);
                    }
                    return -2;
                }
                return -3;
            }
            if(!string.IsNullOrEmpty(str)) {
                try {
                    string[] strArray = str.Split(QTUtility.SEPARATOR_CHAR);
                    Version version = new Version(strArray[0]);
                    if((version > QTUtility.CurrentVersion) || (((strArray.Length == 1) && (version == QTUtility.CurrentVersion)) && (QTUtility.BetaRevision.Major > 0))) {
                        msg = version.ToString();
                        return 2;
                    }
                    if(strArray.Length > 2) {
                        int num = 0;
                        try {
                            Version version2 = new Version(strArray[1]);
                            Version version3 = new Version(strArray[2]);
                            if((version2 <= QTUtility.CurrentVersion) && (!(version2 == QTUtility.CurrentVersion) || (version3 <= QTUtility.BetaRevision))) {
                                goto Label_01AB;
                            }
                            if(version3.Major == 0) {
                                msg = strArray[1] + " Alpha " + version3.Minor;
                            }
                            else {
                                msg = strArray[1] + " Beta " + version3.Major;
                            }
                            num = 1;
                        }
                        catch {
                        }
                        return num;
                    }
                Label_01AB:
                    return 0;
                }
                catch(FormatException) {
                }
            }
            return -4;
        }

        private static bool DayHasCome() {
            using(RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Quizo\QTTabBar")) {
                if(key != null) {
                    long ticks = QTUtility2.GetRegistryValueSafe(key, "LastChecked", -1L);
                    if((DateTime.MinValue.Ticks < ticks) && (ticks < DateTime.MaxValue.Ticks)) {
                        TimeSpan span = (DateTime.Now - new DateTime(ticks));
                        return (span.Days > 5);
                    }
                }
            }
            SaveLastCheck();
            return false;
        }

        private static void SaveLastCheck() {
            using(RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Quizo\QTTabBar", true)) {
                if(key != null) {
                    key.SetValue("LastChecked", DateTime.Now.Ticks, RegistryValueKind.QWord);
                }
            }
        }

        private static void ShowMsg(int code, string strOptional) {
            string text = string.Empty;
            switch(code) {
                case -4:
                    text = "Server returned wrong strings.";
                    break;

                case -3:
                    text = "Server not found.";
                    break;

                case -2:
                    text = "Unknown network error.";
                    break;

                case -1:
                    text = "Version file not found (404).";
                    break;

                case 0:
                    text = QTUtility.TextResourcesDic["UpdateCheck"][1];
                    break;

                case 1:
                    text = QTUtility.TextResourcesDic["UpdateCheck"][1] + "\n\n" + QTUtility.TextResourcesDic["UpdateCheck"][2] + strOptional;
                    break;

                case 2:
                    if(DialogResult.OK == MessageBox.Show(string.Format(QTUtility.TextResourcesDic["UpdateCheck"][0], strOptional), strMsgCaption, MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk)) {
                        try {
                            Process.Start(Resources_String.SiteURL);
                        }
                        catch {
                        }
                    }
                    return;
            }
            MessageBox.Show(text, strMsgCaption, MessageBoxButtons.OK, (code < 0) ? MessageBoxIcon.Hand : MessageBoxIcon.Asterisk);
        }
    }
}
