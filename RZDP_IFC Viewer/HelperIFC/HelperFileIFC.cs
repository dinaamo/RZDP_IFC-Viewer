using Microsoft.Win32;

internal class HelperFileIFC
{
    private static string path;

    public static string OpenIFC_File()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();

        openFileDialog.Filter = "IFC Files|*.ifc;*.ifcxml";

        bool? dialog = openFileDialog.ShowDialog();

        if (dialog == true)
        {
            return openFileDialog.FileName;
        }
        else
        {
            return null;
        }
    }

    public static string[] SelectDocument()

    {
        OpenFileDialog openFileDialog = new OpenFileDialog();

        openFileDialog.Filter = "PDF Files|*.pdf";

        openFileDialog.Multiselect = true;

        bool? dialog = openFileDialog.ShowDialog();

        if (dialog == true)
        {
            return openFileDialog.FileNames;
        }
        else
        {
            return new string[0];
        }
    }

    public static string SaveAsIFC_File(string format)
    {
        if (format != "ifc" && format != "ifcxml")
        { return null; }

        SaveFileDialog saveFileDialog = new SaveFileDialog();

        saveFileDialog.Filter = $"IFC Files|*.{format}";

        bool? dialog = saveFileDialog.ShowDialog();

        if (dialog == true)
        {
            return saveFileDialog.FileName;
        }
        else
        {
            return null;
        }
    }
}