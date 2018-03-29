using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SalesForceMap.Argumen
{
    public class Argumen
    {
    }

    public class DatePilihArgs : EventArgs
    {
        public string TglSekarang { get; set; }
        public string TglBesok { get; set; }
        public DatePilihArgs(string tglsekarang, string tglbesok)
        {
            this.TglSekarang = tglsekarang;
            this.TglBesok = tglbesok;
        }
    }

    public class SalesPilihArgs : EventArgs
    {
        public string KodeSales { get; set; }
        public string NamaSales { get; set; }
        public SalesPilihArgs(string kodesales,string namasales)
        {
            this.KodeSales = kodesales;
            this.NamaSales = namasales;
        }
    }

    public class KodenotaPilihArgs : EventArgs
    {
        public string Kodenota { get; set; }
        public string Keterangan { get; set; }
        public int JmlTerkunjungi { get; set; }
        public int JmlKunjungan { get; set; }
        public KodenotaPilihArgs(string kodenota,string keterangan,int jmlterkunjungi,int jmlkunjungan)
        {
            this.Kodenota = kodenota;
            this.Keterangan = keterangan;
            this.JmlTerkunjungi = jmlterkunjungi;
            this.JmlKunjungan = jmlkunjungan;
        }
    }
}
