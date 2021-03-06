using System;
using System.Globalization;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;
using System.Data.SqlClient;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Text;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;

[assembly: CLSCompliant(true)]
namespace MHPCoreClassLib {

	/// <summary>
	/// Summary description for StringFunc.
	/// </summary>

	public class StringFuncs{

		/// <summary>
		/// Required designer variable.
		/// </summary>
		
		public StringFuncs(){
			/// <summary>
			/// Required for Windows.Forms Class Composition Designer support
			/// </summary>

		}

        public static string GiveMonthName(int i) {
            return GiveMonthName(i, 0);
        }

		public static string GiveMonthName(int i, int len){
			string[] names = {"","January","February","March","April","May","June","July","August","September","October","November","December"};
			string[] names2 = {"","Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"};
			if(i < 13){
				if(len == 0){
					return names[i];
				}else{
					return names2[i];
				}
			}else{
				return "Invalid date int";
			}
		}
		
		public static bool IsValidEmail(string email){
            //bool isLenientMatch = false;
            //string patternLenient = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
            //Regex reLenient = new Regex(patternLenient);
            //try {
            //    isLenientMatch = reLenient.IsMatch(email);
            //} catch (Exception ex) { 
            
            //}
            return IsValidEmail(email, false);
		}

        public static bool IsValidEmail(string email, bool strict) {
            bool isMatch = false;
            string patternLenient = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
            string patternStrict = @"^(([^<>()[\]\\.,;:\s@\""]+"
               + @"(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@"
               + @"((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"
               + @"\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+"
               + @"[a-zA-Z]{2,}))$";
            Regex reLenient = new Regex(patternLenient);
            Regex reStrict = new Regex(patternStrict);
            try {
                if (strict) {
                    isMatch = reStrict.IsMatch(email);
                } else {
                    isMatch = reLenient.IsMatch(email);               
                }
            } catch (Exception ex) {

            }
            return isMatch;
        }

        public static string CleanFileAndPath(string fn) {
            string input = fn + "";
            input = input.Replace('\\','/');
            string[] arr = input.Split('/');
            return CleanFileName(arr[arr.Length-1]);
        }

        public static string CleanFilePath(string fn) {
            string input = fn + "";
            input = input.Replace('\\', '/');
            string[] arr = input.Split('/');
            return arr[arr.Length - 1];
        }

        public static string CleanFileName(string fn) {
            StringBuilder sb = new StringBuilder();
            char[] arr = null;
            int k = 0;
            if (fn.Trim().Length > 0) {
                arr = fn.Trim().ToLower().ToCharArray();
                for (int i = 0; i < arr.Length; i++) {
                    k = Convert.ToInt32(arr[i]);
                    // 97-122 lowercase letters 45 = "-" 46 = "." 48-57 numbers
                    if ((k > 96 && k < 123) || (k == 45) || (k == 46) || (k > 47 && k < 58)) {
                        sb.Append(Convert.ToChar(k));
                    }
                }
            }
            return sb.ToString();
        }

        public static string RemoveIllegalUriCharacters(string s) {
            StringBuilder sb = new StringBuilder();
            char[] chars1 = Path.GetInvalidFileNameChars();
            char[] chars2 = Path.GetInvalidPathChars();
            char[] arr = null;
            int k = 0;
            int j = 0;
            bool isokay = true;

            if (s.Trim().Length > 0) {
                arr = s.Trim().ToCharArray();
                for (int i = 0; i < arr.Length; i++) {
                    isokay = true;
                    k = Convert.ToInt32(arr[i]);

                    for (int m = 0; m < chars1.Length; m++) {
                        j = Convert.ToInt32(chars1[m]);
                        if (k == j) {
                            isokay = false;
                            break;
                        }
                    }
                    if (isokay) {
                        for (int m = 0; m < chars2.Length; m++) {
                            j = Convert.ToInt32(chars2[m]);
                            if (k == j) {
                                isokay = false;
                                break;
                            }
                        }
                    }
                    if (isokay) {
                        sb.Append(Convert.ToChar(k));
                    }
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// Allows only Uppercase alphas, Lowercase alphas, numbers 0 - 9, and then ! # $ % & ' * + - / = ? ^ _ ` { | } ~
        /// </summary>
        /// <param name="fn"></param>
        /// <returns></returns>
        public static string CleanFormFieldEmail(string fn) {
            StringBuilder sb = new StringBuilder();
            char[] arr = null;
            int k = 0;
            arr = fn.ToCharArray();
            for (int i = 0; i < arr.Length; i++) {
                k = Convert.ToInt32(arr[i]);
                // 65 - 90[Uppercase alphas], 97-122[Lowercase alphas], 48-57 [numbers 0 - 9], 45 [-], 46[.], 64[@], 40[(], 41[)], 39['], 44[,], 95[_], 33[!], 35[#]
                if ((k > 64 && k < 91) || (k > 96 && k < 123) || (k > 47 && k < 58) || (k == 33) || (k > 34 && k < 40) || (k == 42) || (k == 43) || (k == 45) || (k == 46) || (k == 47) || (k == 61) || (k == 63) || (k == 64) || (k > 93 && k < 97) || (k > 122 && k < 127)) {
                    sb.Append(Convert.ToChar(k));
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Allows only Uppercase alphas, Lowercase alphas, numbers 0 - 9, and then -.@()',_
        /// </summary>
        /// <param name="fn"></param>
        /// <returns></returns>
        public static string CleanFormField(string fn) {
            // foreign language chars:
            /*
             * 192 - 214 | 216 - 246 | 248 - 304 | 308 - 448 
             * */
            StringBuilder sb = new StringBuilder();
            char[] arr = null;
            int k = 0;
            arr = fn.ToCharArray();
            for (int i = 0; i < arr.Length; i++) {
                k = Convert.ToInt32(arr[i]);
                // 65 - 90[Uppercase alphas], 97-122[Lowercase alphas], 48-57 [numbers 0 - 9], 45 [-], 46[.], 64[@], 40[(], 41[)], 39['], 44[,], 95[_]
                if ((k > 64 && k < 91) || (k > 96 && k < 123) || (k == 32) || (k == 39) || (k == 40) || (k == 41) || (k == 44) || (k == 45) || (k == 46) || (k > 47 && k < 58) || (k == 64) || (k == 95) || (k > 191 && k < 215) || (k > 215 && k < 247)  || (k > 247 && k < 305) ||(k > 307 && k < 449)) {
                    sb.Append(Convert.ToChar(k));
                }
            }
            return sb.ToString();
        }

		public static string CleanUserName(string fn){
			char[] arr = null;
			int k = 0;
			fn = fn.ToLower();
			arr = fn.ToCharArray();
			StringBuilder sb = new StringBuilder();
			for(int i=0;i<arr.Length;i++){
				k = Convert.ToInt32(arr[i]);
				// 97-122 lowercase letters 45 = "-" 46 = "." 48-57 numbers
//				if((k > 64 && k < 91)||(k > 96 && k < 123)||(k == 45)||(k == 46) || (k > 47 && k < 58)){
//					sb.Append(Convert.ToChar(k)); 
//				}
				if((k > 96 && k < 123)||(k > 47 && k < 58)){
					sb.Append(Convert.ToChar(k)); 
				}
			}
			return sb.ToString();
		}

		public static string getFolderDate() {
			return DateTime.Now.ToString("yyyy-mm-dd");
		}

		public static string CleanNonAsciiChars(string str){
			str = str.Replace("’", "'");
			str = str.Replace('”','"');
			str = str.Replace('“','"');
			return str;		
		}

		public static  string encSql(string str,int n){
			str = str.Replace("'","''");
			str = str.Trim();
			if(str.Length > n){
				str = str.Substring(0,n);
			}
			return str;
		}

        public static string encSqlP(string str, int n) {
            str = str.Trim();
            if (str.Length > n) {
                str = str.Substring(0, n);
            }
            return str;
        }

        public static string RemoveAllFolders(string str) { 
            str = str.Replace('/','\\');
            int num = str.LastIndexOf('\\');
            if (num > -1 && num < str.Length) {
                return str.Substring(num+1);
            } else {
                return str;
            }
        }

		public static string RemoveExtention(string str){
			int i = 0;
			i = str.LastIndexOf(".");
			if(i > 0){
				str = str.Substring(0,i);
			}
			return str;
		}
        
		public string CutLastChar(string searchstring, char searchchar){
			if(searchstring != null && searchstring != ""){
				if(searchstring.LastIndexOf(searchchar) == searchstring.Length-1)
					searchstring = searchstring.Substring(0,searchstring.Length-1);
			}
			return searchstring;
		}

		public static string SQLinput(string str,int n){
			str = str.Trim();
			str = str.Replace("'","''");
			if(str.Length > n){
				str = str.Substring(0,n);
			}
			return str;
		}
 
		public static string CheckHref(string href){
			if(href.IndexOf("http://") == -1 && href.IndexOf("https://")  == -1 && href.IndexOf("mailto:")  == -1){
				href = "http://" + href;
			}
			return href;		
		}
        
		public static string StripScripts(string strText, int n, int sql){
			string tmpString ;
			string le = "";
			string ri = "";
			int en = -1;
			tmpString = strText.Trim();
			
			tmpString = tmpString.Replace("<SCRIPT","<script");
			tmpString = tmpString.Replace("</SCRIPT>","</script>");
			int st = tmpString.IndexOf("<script");
			if(st > -1){
				while (st > -1){
					en = tmpString.IndexOf("</script>",st);
					
					le = tmpString.Substring(0,st);
					ri = tmpString.Substring(en + 8);
					tmpString = le + ri;
					st = tmpString.IndexOf("<script");
				}
			}
			while(tmpString.IndexOf("  ") > -1){
				tmpString = tmpString.Replace("  "," ");
			}
			

			//<a href="JavaScript TODO
			if(sql == 1){
				tmpString = tmpString.Replace("'","''");
			}
			if(tmpString.Length > n){
				tmpString = tmpString.Substring(0,n);
			}
			return tmpString;
		}

        public static string StripHtml(string strHtml) {
            //Strips the HTML tags from strHTML
            System.Text.RegularExpressions.Regex objRegExp;
            string strOutput = "";
            try {
                objRegExp = new System.Text.RegularExpressions.Regex("<(.|\n)+?>");
                // Replace all tags with a space, otherwise words either side
                // of a tag might be concatenated
                strOutput = objRegExp.Replace(strHtml, " ");
                strOutput = strOutput.Replace("\r\n", " ");
                // Replace all < and > with &lt; and &gt;
                strOutput = strOutput.Replace("<", "&lt;");
                strOutput = strOutput.Replace(">", "&gt;");
                strOutput = strOutput.Replace("’", "'");
            } catch (Exception ex) {
                strOutput = strHtml;
            }
            return strOutput;
        }

        public static string StripHtmlTags(string Input, string[] AllowedTags) {
            Regex StripHTMLExp = new Regex(@"(<\/?[^>]+>)");
            string Output = Input;

            foreach (Match Tag in StripHTMLExp.Matches(Input)) {
                string HTMLTag = Tag.Value.ToLower();
                bool IsAllowed = false;

                foreach (string AllowedTag in AllowedTags) {
                    int offset = -1;

                    // Determine if it is an allowed tag
                    // "<tag>" , "<tag " and "</tag"
                    if (offset != 0) offset = HTMLTag.IndexOf('<' + AllowedTag + '>');
                    if (offset != 0) offset = HTMLTag.IndexOf('<' + AllowedTag + ' ');
                    if (offset != 0) offset = HTMLTag.IndexOf("</" + AllowedTag);

                    // If it matched any of the above the tag is allowed
                    if (offset == 0) {
                        IsAllowed = true;
                        break;
                    }
                }

                // Remove tags that are not allowed
                if (!IsAllowed) Output = ReplaceFirst(Output, Tag.Value, "");
            }

            return Output;
        }

        public static string StripHtmlTagsAndAttributes(string Input, string[] AllowedTags) {
            /* Remove all unwanted tags first */
            string Output = StripHtmlTags(Input, AllowedTags);

            
            /* Lambda functions */
            //MatchEvaluator HrefMatch = m => m.Groups[1].Value + "href..;,;.." + m.Groups[2].Value;
            //MatchEvaluator ClassMatch = m => m.Groups[1].Value + "class..;,;.." + m.Groups[2].Value;
            //MatchEvaluator UnsafeMatch = m => m.Groups[1].Value + m.Groups[4].Value;

            MatchEvaluator HrefMatch = delegate(Match m) { return m.Groups[1].Value + "href..;,;.." + m.Groups[2].Value; };
            MatchEvaluator ClassMatch = delegate(Match m) { return m.Groups[1].Value + "class..;,;.." + m.Groups[2].Value; };
            MatchEvaluator UnsafeMatch = delegate(Match m) { return m.Groups[1].Value + m.Groups[4].Value; };


            /* Allow the "href" attribute */
            Output = new Regex("(<a.*)href=(.*>)").Replace(Output,HrefMatch);
 
            

            /* Allow the "class" attribute */
            Output = new Regex("(<a.*)class=(.*>)").Replace(Output,ClassMatch);
 
            /* Remove unsafe attributes in any of the remaining tags */
            Output = new Regex(@"(<.*) .*=(\'|\""|\w)[\w|.|(|)]*(\'|\""|\w)(.*>)").Replace(Output,UnsafeMatch);
 
            /* Return the allowed tags to their proper form */
            Output = ReplaceAll(Output,"..;,;..", "=");

        return Output;
        }

		public static string BlankToUnderscore(string str){
			str = str.Trim();
			return( str.Replace(" ","_"));
		}

		public static string MakeDirectoryName(string str){
            string outy = "";
            int k1 = 0;
            char[] arr1 = str.ToCharArray();
            for (int i = 0; i < arr1.Length; i++) {
                k1 = Convert.ToInt32(arr1[i]);
                if ((k1 > 47 && k1 < 58) || (k1 > 64 && k1 < 91) || (k1 > 96 && k1 < 123)) {
                    outy = outy + arr1[i];
                }
            }
            return outy;
		}
        
		public static string Capitalise(string str){
            return (str.ToUpper());
		}

        public static string Capitalise2(string str, bool firstonly) {
            StringBuilder sb = new StringBuilder();
            int count = 0;
            string[] arr = str.Split(' ');
            if (firstonly) {
                for (int i = 0; i < arr.Length; i++) {
                    if (arr[i].Length > 0) { 
                        count++;
                        if (count == 1) {
                            sb.Append(CapWord(arr[i]));
                        } else {
                            sb.Append(" " + arr[i].ToLower());
                        }
                    }
                }
            } else {
                for (int i = 0; i < arr.Length; i++) {
                    if (arr[i].Length > 0) {
                        count++;
                        if (count == 1) {
                            sb.Append(CapWord(arr[i]));
                        } else {
                            sb.Append(" " + CapWord(arr[i]));
                        }
                    }
                }
            }

            return (sb.ToString());
        }

        private static string CapWord(string s) {
            string ss = s.Trim();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ss.Length; i++) { 
                if(i == 0){
                    sb.Append(ss.Substring(i, 1).ToUpper());
                }else{
                    sb.Append(ss.Substring(i, 1).ToLower());
                }
            }
            return (sb.ToString());
        }

		public static string CleanDomain(string str){
			str = str.Trim();
			int slash = str.LastIndexOf("\\");
			if(slash==-1){
				return str;
			}else{
				return str.Substring(slash+1);
			}
		}

        public static string CleanFolders(string str){
			str = str.Trim();
			int slash = str.LastIndexOf("/");
			if(slash==-1){
				return str;
			}else{
				return str.Substring(slash+1);
			}
		}

		public static string RandomWord(int size) {
			Random random = new Random();
            return RandomWord(random, size);
		}

        public static string RandomWord(Random random, int size) {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++) {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString();
        }

        public static string RandomWordLower(int size) {
            Random random = new Random();
            return RandomWordLower(random, size);
        }

        public static string RandomWordLower(Random random, int size) {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++) {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString().ToLower();
        }

		public static string RandomNumberString(int size){
            Random random = new Random();
            return RandomNumberString(random, size);
		}

        public static string RandomNumberString(Random random, int size) {
            StringBuilder builder = new StringBuilder();
            Double db = random.NextDouble();
            char ch;
            for (int i = 0; i < size; i++) {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(10 * random.NextDouble() + 48)));
                builder.Append(ch);
            }
            return builder.ToString();
        }

		public static int RandomNumber(int size){
            Random random = new Random();
            return RandomNumber(random, size);
		}

        public static int RandomNumber(Random random, int size) {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++) {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(10 * random.NextDouble() + 48)));
                builder.Append(ch);
            }
            return Convert.ToInt32(builder.ToString());
        }

        public static string ConvertToKB(long str) {
            string output = "";
            long mod = 0;
            string modout = "";
            if (str > 1024000) {
                mod = str % 1024000;
                modout = mod.ToString();
                if (modout.Length > 2) {
                    modout = modout.Substring(0, 2);
                }
                output = Convert.ToInt64(str / 1024000) + "." + modout + " mb";
            } else if (str > 1024) {
                output = Convert.ToInt64(str / 1024) + " kb";
            } else {
                output = str + " b";
            }
            return (output);
        }

		public static string ConvertToKB(int str){
			string output = "";
			int mod = 0;
			string modout = "";
			if( str > 1024000){
				mod = str % 1024000;
				modout = mod.ToString();
				if(modout.Length > 2){
					modout = modout.Substring(0,2);
				}
				output = Convert.ToInt32(str/1024000) + "."+ modout +" mb";
			}else if( str > 1024 ){
				output = Convert.ToInt32(str/1024) + " kb";
			}else{
				output = str + " b";
			}
			return( output);
		}

		public static string GiveTimeDateString(){
			DateTime date = DateTime.Now;
			return(date.ToString("dd-MM-yyyy")+" "+date.ToLongTimeString());		
		}

		public static string GetMonth(int i){
            return GiveMonthName(i, 0);
		}

		public static string PreparePageName(string pgname){
			pgname = pgname.ToLower();
			char[] letters = pgname.ToCharArray();
			string final = "";
			for(int i=0;i<letters.Length;i++){
				if((Convert.ToInt32(letters[i]) > 96 && Convert.ToInt32(letters[i]) < 123) || (Convert.ToInt32(letters[i]) > 47 && Convert.ToInt32(letters[i]) < 58)){
					final += letters[i].ToString();
				}
			}
			return(final);
		}

		public static string BoolToIntString(bool chkvalue){
			string outy = "";
			if(chkvalue==true){
				outy = "1";
			}else{
				outy = "0";
			}
			return outy;
		}

		public static string FixUrl(string str){
			string lstr = str.ToLower();
			string outy = "";
			if(lstr.Substring(0,8) == "https://"){
				outy = lstr;
			}else if(lstr.Substring(0,7) == "http://"){
				outy = lstr;
			}else{
				outy = "http://" + lstr;
			}
			return outy;
		}

		public static string GetFileExtention(string path){
			int dot = path.LastIndexOf(".");
			string ext = "";
			if(dot>-1){
				ext = path.ToLower().Substring((dot+1));
			}
			return ext;
		}

        public static string OnlyNumbers(string fn) {
            char[] arr = null;
            int k = 0;
            fn = fn.ToLower();
            arr = fn.ToCharArray();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < arr.Length; i++) {
                k = Convert.ToInt32(arr[i]);
                // 97-122 lowercase letters 45 = "-" 46 = "." 48-57 numbers
                if (k > 47 && k < 58) {
                    sb.Append(Convert.ToChar(k));
                }
            }
            return sb.ToString();
        }

        public static bool Is0To9Only(string s) {
            bool outy = true;
            int k = 0;
            char[] arr = s.ToCharArray();
            for (int i = 0; i < arr.Length; i++) {
                k = Convert.ToInt32(arr[i]);
                if (k > 47 && k < 58) {

                } else {
                    outy = false;
                    break;
                }
            }
            return outy;
        }

        public static string OnlyMathsNumbers(string fn) {
            char[] arr = null;
            int k = 0;
            fn = fn.ToLower();
            arr = fn.ToCharArray();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < arr.Length; i++) {
                k = Convert.ToInt32(arr[i]);
                // 97-122 lowercase letters 45 = "-" 46 = "." 48-57 numbers
                if ((k > 41 && k < 58) || (k == 37) || (k == 60) || (k == 61) || (k == 62)) {
                    sb.Append(Convert.ToChar(k));
                }
            }
            return sb.ToString();
        }

        public static bool CheckPassword(string pass1, string pass2, bool CaseSensitive) {
            string p1 = "";
            string p2 = "";
            if (CaseSensitive == true) {
                p1 = pass1.Trim();
                p2 = pass2.Trim();
            } else {
                p1 = pass1.Trim().ToLower();
                p2 = pass2.Trim().ToLower();
            }

            bool outy = false;
            int k1 = 0;
            int k2 = 0;
            if (p1.Length == p2.Length) {
                outy = true;
                char[] arr1 = p1.ToCharArray();
                char[] arr2 = p2.ToCharArray();
                for (int i = 0; i < arr1.Length; i++) {
                    k1 = Convert.ToInt32(arr1[i]);
                    k2 = Convert.ToInt32(arr2[i]);
                    if (k1 != k2) {
                        outy = false;
                    }
                }
            }
            return outy;
        }

        public static string CreatePassword() {
            Random random = new Random();
            return CreatePassword(random);
        }

        public static string CreatePassword(Random random) {
            StringBuilder sb1 = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            StringBuilder sb3 = new StringBuilder();
            string outy = "";
            double n2 = Math.Floor(4 * random.NextDouble());
            double n1 = 0L;
            string numS = "";
            string numN = "";
            string numSL = "";
            int tempy = 0;
            string[] ULetters = { "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "M", "N", "P", "Q", "R", "S", "T", "V", "W", "X", "Y" };
            string[] LLetters = { "a", "b", "c", "d", "e", "f", "g", "h", "j", "k", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y" };
            string[] Numbers = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            try {
                for (int i = 0; i < 10; i++) {
                    n1 = Math.Floor(ULetters.Length * random.NextDouble());
                    tempy = Convert.ToInt32(n1);
                    sb1.Append(ULetters[tempy]);
                }
                numS = sb1.ToString();

                for (int i = 0; i < 10; i++) {
                    n1 = Math.Floor(LLetters.Length * random.NextDouble());
                    tempy = Convert.ToInt32(n1);
                    sb2.Append(LLetters[tempy]);
                }
                numSL = sb2.ToString();

                for (int i = 0; i < 10; i++) {
                    n1 = Math.Floor(Numbers.Length * random.NextDouble());
                    tempy = Convert.ToInt32(n1);
                    sb3.Append(Numbers[tempy]);
                }
                numN = sb3.ToString();

                //numS = RandomWord(random, 10).ToString();
                //numN = RandomNumberString(random, 10).ToString();
                //numSL = RandomWordLower(random, 10).ToString();



                switch (Convert.ToInt32(n2)) {
                    case 0:
                        outy = numSL.Substring(0, 2) + numN.Substring(2, 3) + numS.Substring(5, 1) + numN.Substring(6, 2);
                        break;
                    case 1:
                        outy = numS.Substring(0, 3) + numN.Substring(3, 1) + numSL.Substring(4, 2) + numN.Substring(6, 2);
                        break;
                    case 2:
                        outy = numSL.Substring(0, 1) + numN.Substring(1, 3) + numS.Substring(4, 3) + numN.Substring(7, 1);
                        break;
                    case 3:
                        outy = numN.Substring(0, 2) + numS.Substring(2, 2) + numN.Substring(4, 1) + numSL.Substring(5, 3);
                        break;
                }
            } catch (Exception ex) {
                outy = ex.ToString().Substring(0, 8);
            }
            return outy;
        }

        //public static string CreatePassword(Random random){
        //    StringBuilder sb1 = new StringBuilder();
        //    StringBuilder sb2 = new StringBuilder();
        //    StringBuilder sb3 = new StringBuilder();
        //    string outy = "";
        //    double n1 = Math.Floor(4 * random.NextDouble());
        //    string numS = "";
        //    string numN = "";
        //    string numSL = "";
        //    int tempy = 0;
        //    string[] ULetters = { "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "M", "N", "P", "Q", "R", "S", "T", "V", "W", "X", "Y" };
        //    string[] LLetters = { "a", "b", "c", "d", "e", "f", "g", "h", "j", "k", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y" };
        //    string[] Numbers = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
			

        //    for (int i = 0; i < 10; i++) {
        //        n1 = Math.Floor(ULetters.Length * random.NextDouble());
        //        tempy = Convert.ToInt32(n1);
        //        sb1.Append(ULetters[tempy]);
        //    }
        //    numS = sb1.ToString();

        //    for (int i = 0; i < 10; i++) {
        //        n1 = Math.Floor(LLetters.Length * random.NextDouble());
        //        tempy = Convert.ToInt32(n1);
        //        sb2.Append(LLetters[tempy]);
        //    }
        //    numSL = sb2.ToString();

        //    for (int i = 0; i < 10; i++) {
        //        n1 = Math.Floor(Numbers.Length * random.NextDouble());
        //        tempy = Convert.ToInt32(n1);
        //        sb3.Append(Numbers[tempy]);
        //    }
        //    numN = sb3.ToString();

        //    //numS = RandomWord(random, 10).ToString();
        //    //numN = RandomNumberString(random, 10).ToString();
        //    //numSL = RandomWordLower(random, 10).ToString();

           
            
        //    switch(Convert.ToInt32(n1)){
        //        case 0:
        //            outy = numSL.Substring(0,2) + numN.Substring(2,3) + numS.Substring(5,1) + numN.Substring(6,2);
        //            break;
        //        case 1:
        //            outy = numS.Substring(0,3) + numN.Substring(3,1) + numSL.Substring(4,2) + numN.Substring(6,2) ;
        //            break;
        //        case 2:
        //            outy = numSL.Substring(0,1) + numN.Substring(1,3) + numS.Substring(4,3) + numN.Substring(7,1) ;
        //            break;
        //        case 3:
        //            outy = numN.Substring(0,2) + numS.Substring(2,2) + numN.Substring(4,1) + numSL.Substring(5,3) ;
        //            break;
        //    }
        //    return outy;
        //}
		
		public static bool IsDate(int year, int month, int day) {
			bool outy = true;
			int[] leaps = {2008, 2012, 2016, 2020, 2024, 2028, 2032, 2036, 2040, 2044, 2048, 2052, 2056, 2060, 2064, 2068, 2072, 2076, 2080, 2084, 2088, 2092, 2096};
			               
			int[] d31 = {1,3,5,7,8,10,12};
			int[] d30 = {4,6,9,11};
			bool leap = false;
			
			if(month == 2){
				for(int i=0;i<leaps.Length;i++){
					if(year == leaps[i]){
						leap = true;
					}
				}
				if(leap == true){
					if(day > 29){
						outy = false;
					}
				}else{
					if(day > 28){
						outy = false;
					}
				}
			}else{
				for(int i=0;i<d30.Length;i++){
					if(month == d30[i]){
						if(day > 30){
							outy = false;
						}
					}
				}
			}
			return outy;
		}

		public static string base64Encode(string data) {
			string encodedData = "";
			try {
				byte[] encData_byte = new byte[data.Length];
				encData_byte = System.Text.Encoding.UTF8.GetBytes(data);
				encodedData = Convert.ToBase64String(encData_byte);
			} catch (Exception e) {
                
			}
			return encodedData;
		}

		public static string base64Decode(string data) {
			string result = "";
			try {
				System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
				System.Text.Decoder utf8Decode = encoder.GetDecoder();

				byte[] todecode_byte = Convert.FromBase64String(data);
				int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
				char[] decoded_char = new char[charCount];
				utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
				result = new String(decoded_char);
                
			} catch (Exception e) {
                
			}
			return result;
		}

        public static string GetFilledInt(int num, int places) {
            string s = num.ToString();
            int slen = s.Length;
            string outy = "";
            if (places > slen) {
                for (int i = 0; i < (places - slen); i++) {
                    outy += "0"; 
                }
            } 
            return outy + s; ;       
        }

        public static string TextBoxToHtml(string s) {
            s = s.Replace("\r\n", "<br />");
            return s;
        }

        public static int ShowCost(int n, int rate) {
            double hours = 0;
            double mins = 0;
            double a60 = 60;
            int outy = 0;
            double cost = 0;
            if (n > 0 && rate > 0) {
                if (n > 59) {
                    mins = n % a60;
                    hours = (n / a60);
                    cost = (hours * rate) + ((mins / a60) * rate);
                } else {
                    mins = n;
                    cost = (mins / a60) * rate;
                }
                outy = Convert.ToInt32(cost);
            } 
            return outy; 
        }

        public static string MinutesToHours(int n) {
            int hours = 0;
            int mins = 0;
            if (n > 59) {
                mins = n % 60;
                hours = Math.Abs(n / 60);
            } else {
                mins = n;
            }
            return hours.ToString() + " hours " + mins.ToString() + " minutes";
        }

        public static string MinutesToHours(double n) {
            double hours = 0;
            double mins = 0;
            if (n > 59) {
                mins = n % 60;
                hours = Math.Floor(n / 60);
            } else {
                mins = n;
            }
            return hours.ToString() + "h " + mins.ToString() + "m";
        }

        public static string NumberToString(int n, int places) {
            string a = n.ToString();
            while (a.Length < places) {
                a = "0" + a;
            }
            return a;
        }

        public static string MakeLink(string Text, string Url, bool NewWindow) {
            string outy = "";
            string newwin = "";
            string urllower = "";
            string href = "";
            string firstchar = "";
            int h = -1;
            int i = -1;
            if (NewWindow == true) newwin = " target=\"_NEW\" ";

            if (Url.Trim().Length > 0) {

                urllower = Url.Trim().ToLower();
                firstchar = urllower.Substring(0,1);
                if (firstchar == "/") {
                    href = Url;
                } else {
                    h = urllower.IndexOf("http://");
                    i = urllower.IndexOf("https://");
                    if (h > -1 || i > -1) {
                        href = Url;
                    } else {
                        href = "http://" + Url;
                    }
                }
                
                if (Text.Length > 0) {
                    outy = "<a href=\"" + href + "\" " + newwin + ">" + Text + "</a>";
                } else {
                    outy = "<a href=\"" + href + "\" " + newwin + ">" + Url + "</a>";
                }
            }
            return outy;
        }

        public static string GetTableTop() {
            return GetTableTopInt("", 0);
        }

        public static string GetTableTopClass(string s) {
            return GetTableTopInt(s, 1);
        }

        public static string GetTableTopId(string s) {
            return GetTableTopInt(s, 2);
        }

        private static string GetTableTopInt(string s, int mode) {
            string outy = "";
            switch (mode) { 
                case 0:
                    outy = "<table cellpadding=\"0\" cellspacing=\"0\" border=\"0\">";
                    break;
                case 1:
                    outy = "<table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" class=\"" + s + "\">";
                    break;
                case 2:
                    outy = "<table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" id=\"" + s + "\">";
                    break;
            }
            return outy;
        }

        public static string Convert2AndHash(string s) {
            StringBuilder sb = new StringBuilder();
            char[] letters = s.ToCharArray();
            int letterint = 0;
            for (int i = 0; i < letters.Length; i++) {
                letterint = Convert.ToInt32(letters[i]);
                if ((letterint == 32) || (letterint > 47 && letterint < 58) || (letterint > 64 && letterint < 91) || (letterint > 96 && letterint < 123)) {
                    sb.Append(Convert.ToChar(letterint));
                } else {
                    sb.Append("&#" + letterint.ToString() + ";");
                }
            }
            return sb.ToString();
        }

        public static string MakePagename(string original, char divider) {
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            string ss = "";
            char cc;
            int k = 0;
            original = original.Replace(divider, ' ');
            string[] words = original.ToLower().Split(' ');

            if (words.Length > 1) {
                for (int m = 0; m < words.Length; m++) {
                    sb2 = new StringBuilder();
                    char[] arr = words[m].ToCharArray();
                    for (int i = 0; i < arr.Length; i++) {
                        k = Convert.ToInt32(arr[i]);
                        if (k > 47 && k < 58) {
                            sb2.Append(Convert.ToChar(k));
                        }

                        if (k > 96 && k < 123) {
                            cc = Convert.ToChar(k);
                            ss = Convert.ToString(cc);
                            if (i == 0) {
                                sb2.Append(ss.ToUpper());
                            } else {
                                sb2.Append(ss);
                            }

                        }
                    }
                    if (m > 0 && sb2.Length > 0) {
                        sb.Append(divider.ToString() + sb2.ToString());
                    } else {
                        sb.Append(sb2.ToString());
                    }
                }
            } else {
                sb = new StringBuilder();
                char[] arr = words[0].ToCharArray();
                for (int i = 0; i < arr.Length; i++) {
                    k = Convert.ToInt32(arr[i]);
                    if (k > 47 && k < 58) {
                        sb.Append(Convert.ToChar(k));
                    }

                    if (k > 96 && k < 123) {
                        cc = Convert.ToChar(k);
                        ss = Convert.ToString(cc);
                        if (i == 0) {
                            sb.Append(ss.ToUpper());
                        } else {
                            sb.Append(ss);
                        }

                    }
                }
            }
            return sb.ToString();
        }

        public static string VPathCombine(string mPart1, string mPart2) {
            string part1 = "";
            string part2 = "";
            string outy = "";

            if (mPart1 != null) {
                part1 = mPart1.Trim().Replace('\\', '/');
            }
            if (mPart2 != null) {
                part2 = mPart2.Trim().Replace('\\', '/');
            }
            if (part2.Trim().Length == 0) {
                outy = part1;
            } else if (part1.Trim().Length == 0) {
                outy = part2;
            } else {
                if (part1.EndsWith("/")) {
                    if (part2.StartsWith("/")) {
                        outy = part1 + part2.Substring(1);
                    } else {
                        outy = part1 + part2;
                    }
                } else {
                    if (part2.StartsWith("/")) {
                        outy = part1 + part2;
                    } else {
                        outy = part1 + part2.Substring(1);
                    }
                }
            }
            return outy;
        }

        public static string MakeTextSummary(string s, int charcount) {
            string ss = StripHtml(s);
            string s2 = ss;
            int index = -1;
            try {
                if (ss.Length > charcount) {
                    ss = ss.Substring(0, charcount);
                    index = ss.LastIndexOf(' ');
                    if (index > (charcount / 2)) {
                        ss = ss.Substring(0, index);
                    }
                }
            } catch (Exception ex) {
                ss = s2;
            }
            return ss;
        }

        private static string ReplaceFirst(string haystack, string needle, string replacement) {
            int pos = haystack.IndexOf(needle);
            if (pos < 0) return haystack;
            return haystack.Substring(0, pos) + replacement + haystack.Substring(pos + needle.Length);
        }

        private static string ReplaceAll(string haystack, string needle, string replacement) {
            int pos;
            // Avoid a possible infinite loop
            if (needle == replacement) return haystack;
            while ((pos = haystack.IndexOf(needle)) > 0)
                haystack = haystack.Substring(0, pos) + replacement + haystack.Substring(pos + needle.Length);
            return haystack;
        }

        public static string RemoveLastFolderFromPath(string path) {
            string outy = "";
            int num = 0;
            if (path.Length > 0) {
                if (path.EndsWith("\\")) {
                    outy = path.Substring(0, path.Length - 1);
                } else {
                    outy = path;
                }
                num = outy.LastIndexOf("\\");
                if (num > -1) {
                    outy = outy.Substring(0, num);
                }
            }
            return outy;
        }
        public static string UnEntity(string s) {
            s = UnCharEntity(s);
            s = UnNumberEntity(s);
            return s;
        }

        public static string UnNumberEntity(string s) {
            // punctuation
            s = s.Replace("&#8222;", "„");
            s = s.Replace("&#8364;", "€");
            s = s.Replace("&#8226;", "•");
            s = s.Replace("&#8260;", "⁄");
            s = s.Replace("&#8230;", "…");
            s = s.Replace("&#8212;", "—");
            s = s.Replace("&#8211;", "–");
            s = s.Replace("&#8482;", "™");
            s = s.Replace("&#8220;", "“");
            s = s.Replace("&#8249;", "‹");
            s = s.Replace("&#8216;", "‘");
            s = s.Replace("&#8221;", "”");
            s = s.Replace("&#8250;", "›");
            s = s.Replace("&#8217;", "’");
            s = s.Replace("&#8805;", "≥");
            s = s.Replace("&#8804;", "≤");
            s = s.Replace("&#8734;", "∞");


            // ISO 8859-1 Symbols
            s = s.Replace("&#161;", "¡");
            s = s.Replace("&#162;", "¢");
            s = s.Replace("&#163;", "£");
            s = s.Replace("&#164;", "¤");
            s = s.Replace("&#165;", "¥");
            s = s.Replace("&#166;", "¦");
            s = s.Replace("&#167;", "§");
            s = s.Replace("&#168;", "¨");
            s = s.Replace("&#169;", "©");
            s = s.Replace("&#170;", "ª");
            s = s.Replace("&#171;", "«");
            s = s.Replace("&#172;", "¬");

            s = s.Replace("&#174;", "®");
            s = s.Replace("&#175;", "¯");
            s = s.Replace("&#176;", "°");
            s = s.Replace("&#177;", "±");
            s = s.Replace("&#178;", "²");
            s = s.Replace("&#179;", "³");
            s = s.Replace("&#180;", "´");
            s = s.Replace("&#181;", "µ");
            s = s.Replace("&#182;", "¶");
            s = s.Replace("&#183;", "·");
            s = s.Replace("&#184;", "¸");
            s = s.Replace("&#185;", "¹");
            s = s.Replace("&#186;", "º");
            s = s.Replace("&#187;", "»");
            s = s.Replace("&#188;", "¼");
            s = s.Replace("&#189;", "½");
            s = s.Replace("&#190;", "¾");
            s = s.Replace("&#191;", "¿");
            s = s.Replace("&#215;", "×");
            s = s.Replace("&#247;", "÷");

            // ISO 8859-1 Characters
            s = s.Replace("&#192;", "À");
            s = s.Replace("&#193;", "Á");
            s = s.Replace("&#194;", "Â");
            s = s.Replace("&#195;", "Ã");
            s = s.Replace("&#196;", "Ä");
            s = s.Replace("&#197;", "Å");
            s = s.Replace("&#198;", "Æ");
            s = s.Replace("&#199;", "Ç");
            s = s.Replace("&#200;", "È");
            s = s.Replace("&#201;", "É");
            s = s.Replace("&#202;", "Ê");
            s = s.Replace("&#203;", "Ë");
            s = s.Replace("&#204;", "Ì");
            s = s.Replace("&#205;", "Í");
            s = s.Replace("&#206;", "Î");
            s = s.Replace("&#207;", "Ï");
            s = s.Replace("&#208;", "Ð");
            s = s.Replace("&#209;", "Ñ");
            s = s.Replace("&#210;", "Ò");
            s = s.Replace("&#211;", "Ó");
            s = s.Replace("&#212;", "Ô");
            s = s.Replace("&#213;", "Õ");
            s = s.Replace("&#214;", "Ö");
            s = s.Replace("&#216;", "Ø");
            s = s.Replace("&#217;", "Ù");
            s = s.Replace("&#218;", "Ú");
            s = s.Replace("&#219;", "Û");
            s = s.Replace("&#220;", "Ü");
            s = s.Replace("&#221;", "Ý");
            s = s.Replace("&#222;", "Þ");
            s = s.Replace("&#223;", "ß");
            s = s.Replace("&#224;", "à");
            s = s.Replace("&#225;", "á");
            s = s.Replace("&#226;", "â");
            s = s.Replace("&#227;", "ã");
            s = s.Replace("&#228;", "ä");
            s = s.Replace("&#229;", "å");
            s = s.Replace("&#230;", "æ");
            s = s.Replace("&#231;", "ç");
            s = s.Replace("&#232;", "è");
            s = s.Replace("&#233;", "é");
            s = s.Replace("&#234;", "ê");
            s = s.Replace("&#235;", "ë");
            s = s.Replace("&#236;", "ì");
            s = s.Replace("&#237;", "í");
            s = s.Replace("&#238;", "î");
            s = s.Replace("&#239;", "ï");
            s = s.Replace("&#240;", "ð");
            s = s.Replace("&#241;", "ñ");
            s = s.Replace("&#242;", "ò");
            s = s.Replace("&#243;", "ó");
            s = s.Replace("&#244;", "ô");
            s = s.Replace("&#245;", "õ");
            s = s.Replace("&#246;", "ö");
            s = s.Replace("&#248;", "ø");
            s = s.Replace("&#249;", "ù");
            s = s.Replace("&#250;", "ú");
            s = s.Replace("&#251;", "û");
            s = s.Replace("&#252;", "ü");
            s = s.Replace("&#253;", "ý");
            s = s.Replace("&#254;", "þ");
            s = s.Replace("&#255;", "ÿ");

            /*
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
             */
            return s;
        }

        public static string UnCharEntity(string s) {
            // punctuation

            s = s.Replace("&bdquo;", "„");
            s = s.Replace("&euro;", "€");
            s = s.Replace("&bull;", "•");
            s = s.Replace("&frasl;", "⁄");
            s = s.Replace("&hellip;", "…");
            s = s.Replace("&mdash;", "—");
            s = s.Replace("&ndash;", "–");
            s = s.Replace("&trade;", "™");
            s = s.Replace("&ldquo;", "“");
            s = s.Replace("&lsaquo;", "‹");
            s = s.Replace("&lsquo;", "‘");
            s = s.Replace("&rdquo;", "”");
            s = s.Replace("&rsaquo;", "›");
            s = s.Replace("&rsquo;", "’");
            s = s.Replace("&ge;", "≥");
            s = s.Replace("&le;", "≤");
            s = s.Replace("&infin;", "∞");
            

            // ISO 8859-1 Symbols
            s = s.Replace("&iexcl;", "¡");
            s = s.Replace("&cent;", "¢");
            s = s.Replace("&pound;", "£");
            s = s.Replace("&curren;", "¤");
            s = s.Replace("&yen;", "¥");
            s = s.Replace("&brvbar;", "¦");
            s = s.Replace("&sect;", "§");
            s = s.Replace("&uml;", "¨");
            s = s.Replace("&copy;", "©");
            s = s.Replace("&ordf;", "ª");
            s = s.Replace("&laquo;", "«");
            s = s.Replace("&not;", "¬");
            s = s.Replace("&reg;", "®");
            s = s.Replace("&macr;", "¯");
            s = s.Replace("&deg;", "°");
            s = s.Replace("&plusmn;", "±");
            s = s.Replace("&sup1;", "¹");
            s = s.Replace("&sup2;", "²");
            s = s.Replace("&sup3;", "³");
            s = s.Replace("&acute;", "´");
            s = s.Replace("&micro;", "µ");
            s = s.Replace("&para;", "¶");
            s = s.Replace("&middot;", "·");
            s = s.Replace("&cedil;", "¸");
            s = s.Replace("&ordm;", "º");
            s = s.Replace("&raquo;", "»");
            s = s.Replace("&frac14;", "¼");
            s = s.Replace("&frac12;", "½");
            s = s.Replace("&frac34;", "¾");
            s = s.Replace("&iquest;", "¿");
            s = s.Replace("&times;", "×");
            s = s.Replace("&divide;", "÷");

            // ISO 8859-1 Characters
            s = s.Replace("&Agrave;", "À");
            s = s.Replace("&Aacute;", "Á");
            s = s.Replace("&Acirc;", "Â");
            s = s.Replace("&Atilde;", "Ã");
            s = s.Replace("&Auml;", "Ä");
            s = s.Replace("&Aring;", "Å");
            s = s.Replace("&AElig;", "Æ");
            s = s.Replace("&Ccedil;", "Ç");
            s = s.Replace("&Egrave;", "È");
            s = s.Replace("&Eacute;", "É");
            s = s.Replace("&Ecirc;", "Ê");
            s = s.Replace("&Euml;", "Ë");
            s = s.Replace("&Igrave;", "Ì");
            s = s.Replace("&Iacute;", "Í");
            s = s.Replace("&Icirc;", "Î");
            s = s.Replace("&Iuml;", "Ï");
            s = s.Replace("&ETH;", "Ð");
            s = s.Replace("&Ntilde;", "Ñ");
            s = s.Replace("&Ograve;", "Ò");
            s = s.Replace("&Oacute;", "Ó");
            s = s.Replace("&Ocirc;", "Ô");
            s = s.Replace("&Otilde;", "Õ");
            s = s.Replace("&Ouml;", "Ö");
            s = s.Replace("&Oslash;", "Ø");
            s = s.Replace("&Ugrave;", "Ù");
            s = s.Replace("&Uacute;", "Ú");
            s = s.Replace("&Ucirc;", "Û");
            s = s.Replace("&Uuml;", "Ü");
            s = s.Replace("&Yacute;", "Ý");
            s = s.Replace("&THORN;", "Þ");
            s = s.Replace("&szlig;", "ß");
            s = s.Replace("&agrave;", "à");
            s = s.Replace("&aacute;", "á");
            s = s.Replace("&acirc;", "â");
            s = s.Replace("&atilde;", "ã");
            s = s.Replace("&auml;", "ä");
            s = s.Replace("&aring;", "å");
            s = s.Replace("&aelig;", "æ");
            s = s.Replace("&ccedil;", "ç");
            s = s.Replace("&egrave;", "è");
            s = s.Replace("&eacute;", "é");
            s = s.Replace("&ecirc;", "ê");
            s = s.Replace("&euml;", "ë");
            s = s.Replace("&igrave;", "ì");
            s = s.Replace("&iacute;", "í");
            s = s.Replace("&icirc;", "î");
            s = s.Replace("&iuml;", "ï");
            s = s.Replace("&eth;", "ð");
            s = s.Replace("&ntilde;", "ñ");
            s = s.Replace("&ograve;", "ò");
            s = s.Replace("&oacute;", "ó");
            s = s.Replace("&ocirc;", "ô");
            s = s.Replace("&otilde;", "õ");
            s = s.Replace("&ouml;", "ö");
            s = s.Replace("&oslash;", "ø");
            s = s.Replace("&ugrave;", "ù");
            s = s.Replace("&uacute;", "ú");
            s = s.Replace("&ucirc;", "û");
            s = s.Replace("&uuml;", "ü");
            s = s.Replace("&yacute;", "ý");
            s = s.Replace("&thorn;", "þ");
            s = s.Replace("&yuml;", "ÿ");

            /*
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
            s = s.Replace("", "");
             */
            return s;
        }

        public static string CleanFormField2014(string s) {
            StringBuilder sb = new StringBuilder();
            char[] arr = s.ToCharArray();
            int k = 0;
            for (int i = 0; i < arr.Length; i++) {
                k = Convert.ToInt32(arr[i]);
                if (k == 46 ||
                    k > 47 && k < 58 ||
                    k > 63 && k < 91 ||
                    k > 95 && k < 123 ||
                    k == 127 || k == 138 || k == 142 || k == 154 || k == 156 || k == 158 || k == 159 || k == 163 || k == 165 || k == 902 ||
                    k > 191 && k < 215 ||
                    k > 215 && k < 417 ||
                    k > 451 && k < 684 ||
                    k > 903 && k < 983) {
                    sb.Append(Convert.ToChar(k));
                } else {
                    sb.Append(" ");
                }

                // 48 - 57
                // 64 - 90
                // 96 - 122
                // 127
                // 138, 142, 154, 156, 158, 159, 163, 165, 902
                // 192 - 214
                // 216 - 416
                // 452 - 683
                // 904 - 982
            }
            return sb.ToString();
        }
	}
}

 
