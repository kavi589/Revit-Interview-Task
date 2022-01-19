using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeamHoleCreation
{
    public class BeamHole
    {
        public void FilterBeam()
        {
            try
            {
                ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming);
                var familyInstFilter = new ElementClassFilter(typeof(FamilyInstance));
                var beamFilter = new LogicalAndFilter(filter, familyInstFilter);
                List<Element> elementLst = new FilteredElementCollector(CommonData.Doc).WherePasses(beamFilter)
                        .ToElements() as List<Element>;
                BeamHole beamHole = new BeamHole();
                for (int i = 0; i < elementLst.Count; i++)
                {
                    FamilyInstance beam = elementLst[i] as FamilyInstance;
                    beamHole.CreateHole(beam, CommonData.UIDoc);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void CreateHole(FamilyInstance beam, UIDocument uidoc)
        {
            try
            {
                bool isValid= CheckValidBeam(beam);
                //Element element = uidoc.Document.GetElement(elementId);

                //FamilyInstance beam = element as FamilyInstance;
                if (isValid)
                {
                    var options = new Options();
                    options.ComputeReferences = true;
                    options.View = uidoc.ActiveView;
                   // options.DetailLevel = ViewDetailLevel.Coarse;
                    GeometryElement geometryElement = beam.get_Geometry(options);
                    BeamHole beamHole = new BeamHole();

                    CreateHoleOnBeam(geometryElement, uidoc.Document, beam);
                }
               
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private bool CheckValidBeam(FamilyInstance beam)
        {
            try
            {
                XYZ beamLocationDir = ((beam.Location as LocationCurve).Curve as Line).Direction;
                double length= beam.get_Parameter(BuiltInParameter.INSTANCE_LENGTH_PARAM).AsDouble();
                double unitValue = 0;
                if (CommonData.HoleData.Units.ToLower() == "meter")
                {
                    unitValue = 0.3048;
                }
                else if (CommonData.HoleData.Units.ToLower() == "millimeter")
                {
                    unitValue = 304.8;
                }
                length = Math.Round(length * unitValue,5);
                if (beamLocationDir.IsAlmostEqualTo(beamLocationDir.Normalize()))
                {
                    if (CommonData.Tolerance==0 && length== CommonData.HoleData.BeamLength)
                    {
                        return true;
                    }
                    if ((CommonData.HoleData.BeamLength- CommonData.Tolerance) <=  length && length <= (CommonData.HoleData.BeamLength + CommonData.Tolerance))
                    {
                        return true;
                    }
                   
                }
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        private void CreateHoleOnBeam(GeometryElement geometryElement, Document document, FamilyInstance beam)
        {
            try
            {
                foreach (GeometryObject geoObject in geometryElement)
                {
                    var solid = geoObject as Solid;
                    if (solid != null && solid.GraphicsStyleId!=new ElementId(-1)&& solid?.Faces.Size > 0)
                    {
                        CreateHoleFromSolid(solid, document, beam);
                    }
                    else
                    {
                        GeometryInstance geoInstance = geoObject as GeometryInstance;
                        if (null != geoInstance && geoInstance.IsElementGeometry)
                        {
                            foreach (GeometryObject geometryObject in geoInstance.GetInstanceGeometry())
                            {
                                var Instsolid = geometryObject as Solid;
                                if (Instsolid != null && Instsolid.Faces.Size > 0)
                                {
                                    CreateHoleFromSolid(Instsolid, document, beam);
                                }
                            }
                               // CreateHoleOnBeam(geoInstance.GetInstanceGeometry(), document, beam);
                        }
                    }

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void CreateHoleFromSolid(Solid solid,Document document,FamilyInstance beam)
        {
            using (Transaction transaction = new Transaction(document))
            {
                try
                {
                    XYZ beamDirection = ((beam.Location as LocationCurve).Curve as Line).Direction;
                    XYZ facingDir = beam.FacingOrientation;
                    if (beam.flipFacing())
                        facingDir = -facingDir;
                    double maxAreaValue = solid.Faces.OfType<PlanarFace>().Max(e => e.Area);
                    Face face = solid.Faces.OfType<PlanarFace>().Where(e => Math.Round(e.Area, 5) == Math.Round(maxAreaValue, 5) && e.FaceNormal.IsAlmostEqualTo(facingDir)).First();
                    PlanarFace planarFace = face as PlanarFace;

                    transaction.Start("Hole Creation");

                    List<XYZ> lstPt = new List<XYZ>();
                    EdgeArrayArray loops = planarFace.EdgeLoops;
                    List<Edge> edges = new List<Edge>();
                    foreach (EdgeArray edgeArray in loops)
                    {
                        double minLenght= edgeArray.OfType<Edge>().Min(e => e.ApproximateLength);
                         edges= edgeArray.OfType<Edge>().Where(e => Math.Round(e.ApproximateLength, 5) == Math.Round(minLenght, 5)).ToList();
                        foreach (Edge edge in edges)
                        {
                            lstPt.Add(edge.Tessellate()[0]);
                            lstPt.Add(edge.Tessellate()[edge.Tessellate().Count - 1]);
                        }
                    }
                    XYZ minPt= GetMinPoint(lstPt);
                    Edge shortestEdge = null;
                    edges.ForEach(e =>
                    {
                        bool isMinEdge=  e.Tessellate().Where(x => x.IsAlmostEqualTo(minPt)).Any();
                        if (isMinEdge)
                        {
                             shortestEdge = e;
                        }
                    });
                    //Edge shortestEdge= edges.Where(e => e.Tessellate().Contains(minPt)).FirstOrDefault();
                    Line locationline = ((beam.Location as LocationCurve).Curve as Line);
                    XYZ direction, centrePt; double unitValue = 0;
                    if (CommonData.HoleData.Units.ToLower() == "meter")
                    {
                        unitValue = 3.28084;
                    }
                    else if (CommonData.HoleData.Units.ToLower() == "millimeter")
                    {
                        unitValue = 0.00328084;
                    }
                    GetPoints(shortestEdge, locationline, unitValue, out direction, out centrePt);
                    
                    double startParam = 0.0;
                    double endParam = 2 * Math.PI;

                    //1185
                    for (int i = 0; i < CommonData.HoleData.CellCountValue; i++)
                    {
                        Curve curve = null;
                        if (i == 0)
                        {
                            curve = Ellipse.CreateCurve(centrePt, (CommonData.HoleData.LengthValue/2) * unitValue, (CommonData.HoleData.HeightDiameter / 2) * unitValue, beam.GetTransform().BasisX, beam.GetTransform().BasisZ, startParam, endParam);
                        }
                        else
                        {
                            centrePt = centrePt + (((CommonData.HoleData.LengthValue + CommonData.HoleData.SpacingValue) * unitValue) * direction);
                            curve = Ellipse.CreateCurve(centrePt, (CommonData.HoleData.LengthValue / 2) * unitValue, (CommonData.HoleData.HeightDiameter / 2) * unitValue, beam.GetTransform().BasisX, beam.GetTransform().BasisZ, startParam, endParam);
                        }
                        Plane geomPlane = Plane.CreateByNormalAndOrigin(planarFace.FaceNormal, planarFace.Origin);

                        // Create a sketch plane in current document
                        SketchPlane sketch = SketchPlane.Create(document, geomPlane);
                        CurveArray curveArray = new CurveArray();
                        curveArray.Append(curve);
                        // Create a ModelLine element using the created geometry line and sketch plane
                        document.Create.NewModelCurve(curve, sketch);
                        document.Create.NewOpening(beam, curveArray, Autodesk.Revit.Creation.eRefFace.CenterY);
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    if (transaction.HasStarted())
                    {
                        transaction.RollBack();
                    }
                    throw ex;
                }
            }
        }
        private XYZ GetMinPoint(List<XYZ> lstPt)
        {
            double maxX = lstPt.Max(e => e.X);
            double maxY = lstPt.Max(e => e.Y);
            double maxZ = lstPt.Max(e => e.Z);

            double minX = lstPt.Min(e => e.X);
            double minY = lstPt.Min(e => e.Y);
            double minZ = lstPt.Min(e => e.Z);

            XYZ minPoint = new XYZ(minX, minY, minZ);
            return minPoint;
        }
        private void GetPoints(Edge edge, Line locationline, double unitValue, out XYZ direction, out XYZ centrePt)
        {
            try
            {
                List<XYZ> lstPt = new List<XYZ>();
               
                XYZ firstPt = edge.Tessellate()[0];
                XYZ SecPt = edge.Tessellate()[edge.Tessellate().Count - 1];

                //XYZ maxPoint = new XYZ();
                //XYZ secondPt = new XYZ();
                //if (facingDir.IsAlmostEqualTo(new XYZ(0, 1, 0)) || facingDir.IsAlmostEqualTo(new XYZ(0, -1, 0)))
                //{
                //    maxPoint = new XYZ(minX, maxY, maxZ);
                //    secondPt = new XYZ(maxX, minY, minZ);
                //}
                //else if (facingDir.IsAlmostEqualTo(new XYZ(1, 0, 0)) || facingDir.IsAlmostEqualTo(new XYZ(-1, 0, 0)))
                //{
                //    maxPoint = new XYZ(maxX, minY, maxZ);
                //    secondPt = new XYZ(maxX, maxY, minZ);
                //}

                lstPt.Add(locationline.Tessellate()[0]);
                lstPt.Add(locationline.Tessellate()[edge.Tessellate().Count - 1]);
                XYZ minPoint = GetMinPoint(lstPt);
                XYZ secondPt = lstPt.Where(e => !e.IsAlmostEqualTo(minPoint)).FirstOrDefault();
                direction = (secondPt - minPoint).Normalize();
                XYZ midPoint = (firstPt + SecPt) / 2;

                centrePt = midPoint + ((CommonData.HoleData.EndPostValue * unitValue) * direction) + (((CommonData.HoleData.LengthValue / 2) * unitValue) * direction);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void GetPoints(XYZ facingDir, List<XYZ> lstPt,double unitValue, out XYZ direction, out XYZ centrePt)
        {
            try
            {
                double maxX = lstPt.Max(e => e.X);
                double maxY = lstPt.Max(e => e.Y);
                double maxZ = lstPt.Max(e => e.Z);

                double minX = lstPt.Min(e => e.X);
                double minY = lstPt.Min(e => e.Y);
                double minZ = lstPt.Min(e => e.Z);

                XYZ minPoint = new XYZ(minX, minY, minZ);
                XYZ maxPoint = new XYZ();
                XYZ secondPt = new XYZ();
                if (facingDir.IsAlmostEqualTo(new XYZ(0, 1, 0)) || facingDir.IsAlmostEqualTo(new XYZ(0, -1, 0)))
                {
                    maxPoint = new XYZ(minX, maxY, maxZ);
                    secondPt = new XYZ(maxX, minY, minZ);
                }
                else if (facingDir.IsAlmostEqualTo(new XYZ(1, 0, 0)) || facingDir.IsAlmostEqualTo(new XYZ(-1, 0, 0)))
                {
                    maxPoint = new XYZ(maxX, minY, maxZ);
                    secondPt = new XYZ(maxX, maxY, minZ);
                }

                direction = (secondPt - minPoint).Normalize();
                XYZ midPoint = (minPoint + maxPoint) / 2;

                centrePt = midPoint + ((CommonData.HoleData.EndPostValue * unitValue) * direction) + (((CommonData.HoleData.LengthValue/2) * unitValue) * direction);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
