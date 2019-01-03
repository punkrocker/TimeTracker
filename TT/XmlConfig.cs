namespace XMLHandler
{
    using System;
    using System.Xml;

    public class TXmlConfigHandler
    {
        private string m_filepath;
        private XmlDocument xmldoc;

        public TXmlConfigHandler(string filepath)
        {
            this.m_filepath = filepath;
            this.xmldoc = new XmlDocument();
            this.xmldoc.Load(filepath);
        }

        public string ReadValue(string key)
        {
            string innerText;
            try
            {
                if (this.xmldoc == null)
                {
                    throw new Exception("Read system config file error!");
                }
                innerText = this.xmldoc.SelectSingleNode("//Item[@Key='" + key + "']").InnerText;
            }
            catch (Exception exception)
            {
                throw new Exception("Read Value Error:" + exception.Message);
            }
            return innerText;
        }

        public bool WriteValue( string key , string value )
        {
            bool flag;
            try
            {
                if (this.xmldoc != null)
                {
                    this.xmldoc.SelectSingleNode( "//Item[@Key='" + key + "']" ).InnerText = value;
                    this.xmldoc.Save( this.FilePath );
                    return true;
                }
                flag = false;
            }
            catch (Exception exception)
            {
                throw new Exception( "Write Value Error:" + exception.Message );
            }
            return flag;
        }

        public string ReadValue(string key, bool needCrypt)
        {
            //string value = ReadValue(key);
            //if (needCrypt)
            //    return Crypt.Decrypt(value, CryptKey);
            //else
                return key;
        }


        public string FilePath
        {
            get
            {
                return this.m_filepath;
            }
        }
    }
}

