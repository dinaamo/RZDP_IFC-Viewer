using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACadSharp.IO;
using ACadSharp;
using ACadSharp.Entities;
using System.Windows.Controls;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using CSMath;
using System.IO;
using System.Windows;

namespace RZDP_IFC_Viewer.DWG
{
    /// <summary>
    /// TableEntity
    /// Spline
    /// TextEntity (стили, выравнивание)
    /// MText
    /// Circle
    /// Arc
    /// TextEntity
    /// </summary>

    internal class DWGReader
    {
        CadDocument _cadDoc;
        Vector3D _vectorOffset;

        int _positionZ = 0;

        public DWGReader(string pathDwgDoc, Vector3D vectorOffset)
        {
            _cadDoc = DwgReader.Read(pathDwgDoc);
            if (_cadDoc == null)
            {
                throw new NullReferenceException("Ошибка чтения dwg файла");
            }

            _vectorOffset = vectorOffset;
        }

        public DWGReader(string pathDwgDoc)
        {
            _cadDoc = DwgReader.Read(pathDwgDoc);
            if (_cadDoc == null)
            {
                throw new NullReferenceException("Ошибка чтения dwg файла");
            }

            DeterminateOffsetPoint();
        }

        public IEnumerable<Visual3D> ExtractEntityForHelix()
        {
            foreach (var line in ExtractLine())
            {
                yield return line;
            }

            foreach (var polyline in ExtractPolyline())
            {
                yield return polyline;
            }

            foreach (var text in ExtractText())
            {
                yield return text;
            }

        }

        IEnumerable<Visual3D> ExtractText()
        {
            foreach (IText baseText in _cadDoc.Entities.OfType<IText>())
            {
                TextVisual3D textVisual3D = new TextVisual3D();
                textVisual3D.Position = new Point3D(baseText.InsertPoint.X, baseText.InsertPoint.Y, (baseText.InsertPoint.Z) * _positionZ) - _vectorOffset;
                textVisual3D.Height = baseText.Height;
                textVisual3D.UpDirection = new Vector3D(0, 1, 0);
                textVisual3D.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(baseText.Color.R, baseText.Color.G, baseText.Color.B));
                

                string fontFamilyName = Path.GetFileNameWithoutExtension(baseText.Style.Filename);
                Uri pathToFont = new Uri(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), baseText.Style.Filename));

                textVisual3D.FontFamily = new FontFamily(pathToFont, fontFamilyName);

                if (baseText is TextEntity text)
                {
                    textVisual3D.Angle = text.Rotation * 57.29577951308;
                    textVisual3D.Text = text.Value;
                }
                else if (baseText is MText mText)
                {
                    textVisual3D.Angle = mText.Rotation * 57.29577951308;
                    textVisual3D.Text = mText.Value;
                }

                yield return textVisual3D;
            }

        }



        IEnumerable<Visual3D> ExtractPolyline()
        {
            foreach (IPolyline polyline in _cadDoc.Entities.OfType<IPolyline>())
            {
                IEnumerable<IVector> locations = polyline.Vertices.Select(vr => vr.Location);

                List<Point3D> point3dCollection = locations.OfType<XY>().Select(p2 => new Point3D(p2.X, p2.Y, 0)).
                    Concat(locations.OfType<XYZ>().Select(p3 => new Point3D(p3.X, p3.Y, p3.Z))).ToList();
                
                int countPoint = point3dCollection.Count();
                for (int i = 0; i < countPoint; i++)
                {
                    if((i+1) < countPoint)
                    {
                        LinesVisual3D linesVisual3D = new LinesVisual3D();
                        linesVisual3D.Points = new Point3DCollection
                        { 
                            new Point3D(point3dCollection[i].X, point3dCollection[i].Y, (point3dCollection[i].Z) * _positionZ) - _vectorOffset,
                            new Point3D(point3dCollection[i+1].X, point3dCollection[i+1].Y, (point3dCollection[i+1].Z) * _positionZ) - _vectorOffset
                        };

                        linesVisual3D.Thickness = polyline.Thickness > 1 ? polyline.Thickness : 1;
                        linesVisual3D.Color = System.Windows.Media.Color.FromRgb(polyline.Color.R, polyline.Color.G, polyline.Color.B);
                        ;
                        yield return linesVisual3D;
                    }
                }
            }
        }


        IEnumerable<Visual3D> ExtractLine()
        {
            foreach (Line line in _cadDoc.Entities.OfType<Line>())
            {
                Point3DCollection linePoints = new Point3DCollection
                {
                    new Point3D(line.StartPoint.X - _vectorOffset.X, line.StartPoint.Y - _vectorOffset.Y, line.StartPoint.Z * _positionZ - _vectorOffset.Z),
                    new Point3D(line.EndPoint.X - _vectorOffset.X, line.EndPoint.Y - _vectorOffset.Y, line.EndPoint.Z * _positionZ - _vectorOffset.Z)
                };

                LinesVisual3D linesVisual3D = new LinesVisual3D();
                linesVisual3D.Points = linePoints;

                linesVisual3D.Color = System.Windows.Media.Color.FromRgb(line.Color.R, line.Color.G, line.Color.B);
                yield return linesVisual3D;
            }
        }

        private void DeterminateOffsetPoint()
        {
            var boundingBoxes = _cadDoc.ModelSpace.Entities.Select(ent => ent.GetBoundingBox()).Select(bbx => bbx.Min);
            double averageX = Enumerable.Average(boundingBoxes.Select(xyz => xyz.X));
            double averageY = Enumerable.Average(boundingBoxes.Select(xyz => xyz.Y));
            _vectorOffset = new Vector3D(averageX, averageY, 0);
        }

    }
}
