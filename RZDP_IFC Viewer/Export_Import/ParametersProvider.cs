using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using Editor_IFC;
using Microsoft.Win32;
using RZDP_IFC_Viewer.HelperIFC;
using RZDP_IFC_Viewer.IFC.Model.ModelObjectPropertySet.Base;
using RZDP_IFC_Viewer.IFC.ModelItem;
using RZDP_IFC_Viewer.Infracrucrure.FindObjectException;
using Xbim.Ifc4.Interfaces;
using static System.Net.Mime.MediaTypeNames;

namespace RZDP_IFC_Viewer.Exporter
{
    internal class ParametersProvider
    {
        string _ifcFilePath;
        ModelItemIFCObject _modelItemObject;
        ParametersObjectToExportOrImport _parametersObject;

        string GetOutputFilePath()
        {
            string fileName = "Parameters: " + _parametersObject.ElementName;
            HelperReplaceSymbols.ReplacingSymbols(ref fileName);
            string outputFilePath = Path.Combine(Path.GetDirectoryName(_ifcFilePath), fileName);
            int index = 1;
            while (File.Exists(outputFilePath + ".xml"))
            {
                outputFilePath = Path.Combine(Path.GetDirectoryName(_ifcFilePath), fileName + "_" + index++);
            }
            return outputFilePath + ".xml";
        }

        public ParametersProvider(ModelItemIFCObject modelItemObject)
        {
            _modelItemObject = modelItemObject;
            _ifcFilePath = modelItemObject.Model.FilePath;
            _parametersObject = new ParametersObjectToExportOrImport(modelItemObject);
        }

        public void ExportParametersToXML()
        {
            XmlSerializer xml = new XmlSerializer(typeof(ParametersObjectToExportOrImport));
            string outputFilePath = GetOutputFilePath();

            using (FileStream fs = new FileStream(outputFilePath, FileMode.Create))
            {
                xml.Serialize(fs, _parametersObject);
            }

            MessageBox.Show("Параметры экспортированы в файл:\n" + outputFilePath);
        }

        public ParametersObjectToExportOrImport ImportParametersFromXML()
        {
            XmlSerializer xmlDesterilize = new XmlSerializer(typeof(ParametersObjectToExportOrImport));

            using (FileStream fs = new FileStream(SelectOutputXMLFile(), FileMode.OpenOrCreate))
            {
                if (xmlDesterilize.Deserialize(fs) is ParametersObjectToExportOrImport _parametersObject)
                {
                    return _parametersObject;
                }
                else
                {
                    throw new NullReferenceException("Ошибка чтения файла!");
                }
            }
        }

        string SelectOutputXMLFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Файлы xml|*.xml";
            openFileDialog.InitialDirectory = Path.GetDirectoryName(_ifcFilePath);
            openFileDialog.Title = "Выберите файл параметров";

            if(openFileDialog.ShowDialog() != true)
            {
                throw new ExitOperationException();
            }

            return openFileDialog.FileName;
        }

    }





   
}
