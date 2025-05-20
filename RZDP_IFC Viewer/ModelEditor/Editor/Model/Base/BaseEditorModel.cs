using System.Data;
using RZDP_IFC_Viewer.IFC.Model;
using RZDP_IFC_Viewer.ViewModels;
using IFC_Viewer.IFC.Editor;
using RZDP_IFC_Viewer.IFC.Editor.Base;
using Xbim.Ifc4.Interfaces;

namespace IFC_Viewer.IFC.Base
{
    public abstract class BaseEditorModel : BaseItemModel
    {
        protected BaseEditorModel(ModelIFC modelIFC) : base(modelIFC)
        { }

        public static BaseEditorModel CreateEditor(ModelIFC modelIFC)
        {
            if (modelIFC.IfcStore.SchemaVersion == Xbim.Common.Step21.XbimSchemaVersion.Ifc2X3)
            {
                return new EditorModelIFC2x3(modelIFC);
            }
            else if (modelIFC.IfcStore.SchemaVersion == Xbim.Common.Step21.XbimSchemaVersion.Ifc4 ||
                            modelIFC.IfcStore.SchemaVersion == Xbim.Common.Step21.XbimSchemaVersion.Ifc4x1)
            {
                return new EditorModelIFC4(modelIFC);
            }
            else
            {
                throw new ArgumentException($"Не соответствие схемы ifc");
            }
        }

        public abstract IIfcTable CreateNewIFCTable(DataTable dataTable);

        public abstract IIfcDocumentReference CreateNewIFCDocumentInformation(ModelDocument modelDocument);

        public abstract IIfcRelAssociatesDocument CreateAssociateDocument(IIfcDocumentReference ifcDocumentReference, IEnumerable<IIfcObjectDefinition> ifcObjectDefinitionSet);

        /// <summary>
        /// Удалить ассоциацию документа с объектом
        /// </summary>
        public void RemoveAssociationObjectWithDocument(IIfcObjectDefinition iIfcObjectDefinition, IIfcDocumentReference ifcDocumentReference)
        {
            IEnumerable<IIfcRelAssociatesDocument> ifcRelAssociatesDocument = ModelIFC.IfcStore.Instances.
                                                                                    OfType<IIfcRelAssociatesDocument>().
                                                                                    Where(it => it.RelatingDocument.Equals(ifcDocumentReference)).
                                                                                    Where(it => it.RelatedObjects.Contains(iIfcObjectDefinition));

            foreach (IIfcRelAssociatesDocument ifcRelAssociatesDocumentSet in ifcRelAssociatesDocument)
            {
                ifcRelAssociatesDocumentSet.RelatedObjects.Remove(iIfcObjectDefinition);
                if (ifcRelAssociatesDocumentSet.RelatedObjects.Count == 0)
                {
                    ModelIFC.IfcStore.Delete(ifcRelAssociatesDocumentSet);
                }
            }
        }


        /// <summary>
        /// Удалить ассоциацию при удалении документа
        /// </summary>
        public void DeleteAssociatesDocument(IIfcDocumentReference ifcDocumentReference)
        {
            IEnumerable<IIfcRelAssociatesDocument> ifcRelAssociatesDocumentSet = ModelIFC.IfcStore.Instances.
                                                                                   OfType<IIfcRelAssociatesDocument>().
                                                                                   Where(it => it.RelatingDocument.Equals(ifcDocumentReference));
            foreach (IIfcRelAssociatesDocument ifcRelAssociatesDocument in ifcRelAssociatesDocumentSet)
            {
                ModelIFC.IfcStore.Delete(ifcRelAssociatesDocument);
            }
        
        }


    }
}