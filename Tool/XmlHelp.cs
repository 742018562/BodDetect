using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BodDetect
{
    public  class XmlHelp
    {
        public const string Xmlpath = "D://data2.xml";

        public static void createXml(string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "");
            xmlDoc.AppendChild(node);
            //创建根节点    
            XmlNode root = xmlDoc.CreateElement("StandData");
            xmlDoc.AppendChild(root);

            CreateNode(xmlDoc, root, "time", DateTime.Now.ToString());

            try
            {
                xmlDoc.Save(path);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

            }

        }

        /// <summary>      
        /// 创建节点      
        /// </summary>      
        /// <param name="xmldoc"></param>  xml文档    
        /// <param name="parentnode"></param>父节点      
        /// <param name="name"></param>  节点名    
        /// <param name="value"></param>  节点值    
        ///     
        public static void CreateNode(XmlDocument xmlDoc, XmlNode parentNode, string name, string value)
        {
            XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, name, null);
            node.InnerText = value;
            parentNode.AppendChild(node);
        }

        public static void updatexml(string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);//加载xml文件，文件
            XmlNode xns = xmlDoc.SelectSingleNode("StandData");//查找要修改的节点
            XmlNodeList xnl = xns.ChildNodes;//取出所有的子节点
            foreach (XmlNode xn in xnl)
            {
                if(xn.Name == "time") 
                {
                    xn.InnerText = DateTime.Now.ToString();
                }
                //XmlElement xe = (XmlElement)xn;//将节点转换一下类型
                //if (xe.GetAttribute("类别") == "文学")//判断该子节点是否是要查找的节点
                //{
                //    xe.SetAttribute("类别", "娱乐");//设置新值
                //}
                //else//为了有更明显的效果，所以不管是否是符合条件的子节点，我都给一个操作
                //{
                //    xe.SetAttribute("类别", "文学");
                //}
                //XmlNodeList xnl2 = xe.ChildNodes;//取出该子节点下面的所有元素
                //foreach (XmlNode xn2 in xnl2)
                //{
                //    XmlElement xe2 = (XmlElement)xn2;//转换类型
                //    if (xe2.Name == "price")//判断是否是要查找的元素
                //    {
                //        if (xe2.InnerText == "10.00")//判断该元素的值并设置该元素的值
                //            xe2.InnerText = "15.00";
                //        else
                //            xe2.InnerText = "10.00";
                //    }
                //    //break;//这里为了明显效果 我注释了break,用的时候不用，这个大家都明白的哈
                //}
                ////break;
            }
            xmlDoc.Save(path);//再一次强调 ，一定要记得保存的该XML文件

        }
        //读取Xml文件中的节点元素
        public static string readtext(string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlNode xn = xmlDoc.SelectSingleNode("StandData");

            XmlNodeList xnl = xn.ChildNodes;
            foreach (XmlNode xnf in xnl)
            {
                if (xnf.Name == "time") 
                {
                    return xnf.InnerText;
                }
            }

            return string.Empty;
        }
    }


}
