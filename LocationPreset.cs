using System;
public class LocationPreset
{
    public int LocationID { get; set; }
    public string LocationName { get; set; }

    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }

    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    public bool IsFamily { get; set; }
    public bool IsActive { get; set; }
    public string LocationNotes { get; set; }

    // Helper for your UI or Tooltips
    public string FullAddress => $"{Address1}, {City}, {State} {ZipCode}".Trim(',', ' ');

    // Formats coordinates for your ExifTool command string
    public string ExifLat => Latitude.ToString("F6");
    public string ExifLon => Longitude.ToString("F6");
}

//        String connectionString = @"Server=.\SQLEXPRESS;Database=JournalDB;Trusted_Connection=True;TrustServerCertificate=True;";