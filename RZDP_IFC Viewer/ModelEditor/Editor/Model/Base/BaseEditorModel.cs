using System.Data;
using RZDP_IFC_Viewer.IFC.Model;
using RZDP_IFC_Viewer.ViewModels;
using IFC_Viewer.IFC.Editor;
using RZDP_IFC_Viewer.IFC.Editor.Base;
using Xbim.Ifc4.Interfaces;

namespace IFC_Viewer.IFC.Base
{
    public abstract class BaseEditorModel : BaseModel
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

        //public void DeleteIFCObjectReferenceSelect(IIfcObjectReferenceSelect objectReferenceSelect)
        //{
        //    ModelIFC.IfcStore.Delete(objectReferenceSelect);

            
        //}

        //public void DeleteReferenceToDocument(IIfcDocumentReference documentReference)
        //{
        //    ModelIFC.IfcStore.Delete(documentReference);

        //    IEnumerable<IIfcPropertySet> PropertySetCollection = ModelIFC.IfcStore.Instances.OfType<IIfcObject>().
        //        SelectMany(it => it.IsDefinedBy.
        //        Select(obj => obj.RelatingPropertyDefinition).
        //        OfType<IIfcPropertySet>()).
        //        Where(it => it.HasProperties.Select(pr => pr.GetType() == typeof(IIfcPropertyReferenceValue)) != null).
        //        Where(it => it.HasProperties.Select(pr => ((IIfcPropertyReferenceValue)pr).PropertyReference == documentReference) != null);

        //    foreach (IIfcPropertySet PropertySet in PropertySetCollection)
        //    {
        //        IIfcProperty deletedProperty = PropertySet.HasProperties.
        //            Where(it => it.GetType() == typeof(IIfcPropertyReferenceValue)).
        //            FirstOrDefault(pr => ((IIfcPropertyReferenceValue)pr).PropertyReference == documentReference);

        //        PropertySet.HasProperties.Remove(deletedProperty);
        //    }
        //}
    }
}