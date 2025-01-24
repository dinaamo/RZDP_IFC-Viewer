using System.Data;
using RZDP_IFC_Viewer.IFC.Model;
using RZDP_IFC_Viewer.ViewModels;
using IFC_Viewer.IFC.Base;
using Xbim.Ifc2x3.ExternalReferenceResource;
using Xbim.Ifc2x3.MeasureResource;
using Xbim.Ifc2x3.UtilityResource;
using Xbim.Ifc4.Interfaces;

namespace IFC_Viewer.IFC.Editor
{
    internal class EditorModelIFC2x3 : BaseEditorModel
    {
        public EditorModelIFC2x3(ModelIFC modelIFC) : base(modelIFC)
        {
        }

        public override IIfcTable CreateNewIFCTable(DataTable dataTable)
        {
            IIfcTable ifcTable;

            ifcTable = ModelIFC.IfcStore.Instances.New<IfcTable>(tb =>
            {
                tb.Name = dataTable.TableName;

                IfcTableRow rowIsHeading = ModelIFC.IfcStore.Instances.New<IfcTableRow>(rh =>
                {
                    foreach (object header in dataTable.Columns)
                    {
                        rh.RowCells.Add(new IfcText(Convert.ToString(header)));
                    }
                    rh.IsHeading = true;
                });
                tb.Rows.Add(rowIsHeading);

                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    IfcTableRow row = ModelIFC.IfcStore.Instances.New<IfcTableRow>();

                    foreach (object cell in dataTable.Rows[i].ItemArray)
                    {
                        row.RowCells.Add(new IfcText(Convert.ToString(cell)));
                    }

                    row.IsHeading = false;

                    tb.Rows.Add(row);
                }
            });

            return ifcTable;
        }

        public override IIfcDocumentReference CreateNewIFCDocumentInformation(ModelDocument modelDocument)
        {
            IIfcDocumentInformation ifcDocumentInformation = ModelIFC.IfcStore.Instances.New<IfcDocumentInformation>();

            ifcDocumentInformation.Identification = modelDocument.RelativePath;
            ifcDocumentInformation.Name = modelDocument.NameDocument;
            ifcDocumentInformation.Purpose = ""; //Цель этого документа.
            ifcDocumentInformation.IntendedUse = ""; //Предполагаемое использование этого документа.
            ifcDocumentInformation.CreationTime = modelDocument.CreationTime; //Дата и время первоначального создания документа.
            ifcDocumentInformation.LastRevisionTime = modelDocument.LastRevisionTime; //Дата и время создания данной версии документа.
            ifcDocumentInformation.ElectronicFormat = modelDocument.ElectronicFormat; // «application/pdf» обозначает тип подтипа pdf (Portable Document Format)

            IIfcDocumentReference ifcDocumentReference = ModelIFC.IfcStore.Instances.New<IfcDocumentReference>();
            ifcDocumentReference.Name = modelDocument.NameDocument;
            ifcDocumentReference.ReferencedDocument = ifcDocumentInformation;

            return ifcDocumentReference;
        }
    }
}