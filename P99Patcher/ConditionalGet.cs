using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace P99Patcher
{
    class ConditionalGet
    {
        string userName;
        string password;
        string outputFile;
        string eTag;
        string ifModifiedSince;
        bool downloaded;
        string iurl;
        string ifile;
        string ifolder;

        public ConditionalGet(string url, string file , string folder)
        {
            userName = string.Empty;
            password = string.Empty;
            outputFile = null;
            eTag = null;
            ifModifiedSince = null;
            downloaded = false;

            iurl = url;
            ifile = file;
            ifolder = folder;
        }

        public bool Download()
        {
            if(false==System.IO.Directory.Exists(ifolder))
                System.IO.Directory.CreateDirectory(ifolder);

            string file = ExtractFileNameFromPath(iurl);
            outputFile = System.IO.Path.Combine(ifolder, file);

            downloaded = false;

            bool result = false;
            try
            {
                if(ReadTag())
                    result = ConditionallyRetrieveEntity(iurl, this.eTag, this.ifModifiedSince);
                else
                  result = RetrieveEntity(iurl);
            }
            catch (System.Net.WebException ex)
            {
                if (ex.Response != null)
                {
                    using (HttpWebResponse response = ex.Response as HttpWebResponse)
                    {
                        if (response.StatusCode == HttpStatusCode.NotModified)
                        {
                            return false;
                        }
                        else
                            return false;
                    }
                }
            }

            if(File.Exists(outputFile))
            {
                if (downloaded)
                    WriteTag();
                return true;
            }
			else 
            {
                return false;
            }

            
        }

        private bool ReadTag()
        {
            string tagFile = outputFile + ".etag";

            if (System.IO.File.Exists(tagFile) == false)
                return false;
            
            FileStream fs = null;
            StreamReader sr = null;
            try
            {
                fs = new FileStream(
                tagFile, FileMode.Open, FileAccess.Read, FileShare.None);

                sr = new StreamReader(fs);
                this.eTag = sr.ReadLine();
                this.ifModifiedSince = sr.ReadLine();
                fs.Close();
                sr.Close();
            }
            catch (System.Exception)
            {
                return false;
            }

            return true;
        }

        private bool WriteTag()
        {
            string tagFile = outputFile + ".etag";

            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                fs = new FileStream(
                    tagFile, FileMode.Create, FileAccess.Write, FileShare.None);

                sw = new StreamWriter(fs);
                if (string.IsNullOrEmpty(this.eTag))
                    sw.WriteLine("");
                else
                    sw.WriteLine(this.eTag);
                if (string.IsNullOrEmpty(this.ifModifiedSince))
                    sw.WriteLine("");
                else
                    sw.WriteLine(this.ifModifiedSince);
                sw.Flush();
                fs.Close();
                sw.Close();
            }
            catch(System.Exception)
            {
                return false;
            }

            return true;
        }

        private bool RetrieveEntity(string Uri)
        {
            // Check inbound parameters
            if (string.IsNullOrEmpty(Uri))
            {
                throw new ArgumentOutOfRangeException("Uri");
            }
            // Create the request that we'll use to retrieve the Entity.
            WebRequest request = HttpWebRequest.Create(Uri);
            request.Method = "GET";
            request.Credentials = new NetworkCredential(userName, password);

            // Next, Get the response.
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                // Then, read in the response body.
                bool result = ReadResponse(response);
                if (response.StatusCode != HttpStatusCode.OK)
                    return false;
            }

            return true;
        }

        private void SetUserName(string userName, string password)
        {
            this.userName = userName;
            this.password = password;
        }

        private bool ConditionallyRetrieveEntity(string Uri, string eTag, string ifModifiedSince)
        {
            // Check inbound parameters
            if (string.IsNullOrEmpty(Uri))
            {
                throw new ArgumentOutOfRangeException("Uri");
            }
            // Create the request that we'll use to retrieve the Entity.
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Uri);
            request.Method = "GET";

            // Set the IfNoneMatch with the eTag we have just stored
            request.Headers[HttpRequestHeader.IfNoneMatch] = this.eTag;
            // Set the IfModifiedSince with the last modified date we have just stored
            DateTime dt = DateTime.Parse(this.ifModifiedSince);
            request.IfModifiedSince = dt;
            request.Credentials = new NetworkCredential(userName, password);

            // Next, Get the response.
            bool result = false;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                // Then, read in the response body.
                result = ReadResponse(response);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return false;
                }

            }

            return result;
        }

        private bool ReadResponse(HttpWebResponse response)
        {
            FileStream fs = null;
            Stream strm = null;
        
            try
            {
        		strm = response.GetResponseStream();

                // store the eTag and last modified date of the file to be used in the http conditional get
                this.eTag = response.Headers[HttpResponseHeader.ETag];
                this.ifModifiedSince = response.Headers[HttpResponseHeader.LastModified];

		        fs = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None);

		        const int ArrSize = 10000;

		        Byte[] barr = new Byte[ArrSize];

		        while(true) 
		        {
			        int Result = strm.Read(barr, 0, ArrSize); 

			        if (Result == -1 || Result == 0)
				        break;

			        fs.Write(barr, 0, Result);
		        }

		        fs.Flush();
                fs.Close();
                strm.Close();
                response.Close();
            }
            catch(System.Net.WebException)
	        {
                if (File.Exists(outputFile))
                    File.Delete(outputFile);
                return false;
            }

            downloaded = true;
            return true;
        }

        private string ExtractFileNameFromPath(string szPath)
        {
            if (string.IsNullOrEmpty(szPath))
                return string.Empty;

            int pos = szPath.LastIndexOf('\\');
            if (pos == 0 || pos == -1)
            {
                pos = szPath.LastIndexOf('/');
                if (pos == 0 || pos == -1)
                    return string.Empty;
                else
                    return szPath.Substring(pos + 1, szPath.Length - (pos + 1));
            }
            else
                return szPath.Substring(pos + 1, szPath.Length - (pos + 1));
        }
    }
}
