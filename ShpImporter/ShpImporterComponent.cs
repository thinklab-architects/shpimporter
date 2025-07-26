using System;
using System.IO;
using System.Collections.Generic;
using Grasshopper.Kernel;
using NetTopologySuite.Features;
using NetTopologySuite.IO.ShapeFile.Extended;
using NetTopologySuite.Geometries;

public class ShpImporterComponent : GH_Component
{
    public ShpImporterComponent()
      : base("ShpImporter", "ShpImporter",
          "匯入 GIS shp 檔案並取得點、線及屬性資訊",
          "GIS", "Import")
    {
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddTextParameter("Shp檔路徑", "Path", "shp檔案完整路徑", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddGenericParameter("幾何", "Geom", "點或線的幾何資料", GH_ParamAccess.list);
        pManager.AddGenericParameter("屬性", "Attr", "shp檔案中的屬性資訊", GH_ParamAccess.list);
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
        string shpPath = null;
        if (!DA.GetData(0, ref shpPath) || !File.Exists(shpPath))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "請提供有效的shp檔案路徑");
            return;
        }

        var geometries = new List<object>();
        var attributes = new List<Dictionary<string, object>>();

        using (var reader = new ShapeDataReader(shpPath))
        {
            foreach (var feature in reader.ReadByMBRFilter(reader.ShapefileBounds))
            {
                var geom = feature.Geometry;
                if (geom is Point || geom is LineString || geom is MultiPoint || geom is MultiLineString)
                {
                    geometries.Add(geom);
                    var attrDict = new Dictionary<string, object>();
                    foreach (var name in feature.Attributes.GetNames())
                    {
                        attrDict[name] = feature.Attributes[name];
                    }
                    attributes.Add(attrDict);
                }
            }
        }

        DA.SetDataList(0, geometries);
        DA.SetDataList(1, attributes);
    }

    public override Guid ComponentGuid
    {
        get { return new Guid("A1B2C3D4-E5F6-47A8-9B0C-1234567890AB"); }
    }
}
