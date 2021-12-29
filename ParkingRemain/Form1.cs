using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ParkingRemain.Models.FEDS;
using ParkingRemain.Models.ParkingLotInfo;
using System.IO;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace ParkingRemain
{
    public partial class Form1 : Form
    {
        private FEDSEntities _fdb = new FEDSEntities();
        private ParkingLotInfoEntities _pdb = new ParkingLotInfoEntities();
        DateTime dt = DateTime.Now;

        public Form1()
        {
            InitializeComponent();
        }

        public class SpaceInfo_Result
        {
            public int status { get; set; }
            public string remark { get; set; }
            public List<spaceList_Result> spaceLists { get; set; }
        }
        //車位資訊
        public class spaceList_Result
        {
            public string mallId { get; set; }
            public string car { get; set; }
            public string motor { get; set; }
        }

        public class spacetxt
        {
           public result results { get; set; }
        }
        public class result
        {
            public List<parking_infos> parking_info { get; set; }
        }
        public class parking_infos
        {
            public string code { get; set; }
            public int motorcycle { get; set; }
            public int car { get; set; }
            public string update { get; set; }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SpaceInfo_Result _result = new SpaceInfo_Result();
            _result.spaceLists = new List<spaceList_Result>();

            try
            {
                _result = Parking_get_remain();
                if(_result.status == 1)
                {
                    foreach (var s in _result.spaceLists)
                    {
                        int mallId = Convert.ToInt32(s.mallId);
                        ParkinglotInfo _p = _fdb.ParkinglotInfo.Where(o => o.MallId == mallId).FirstOrDefault();
                        _p.CarParkinglot = s.car;
                        _p.MotocycleParkinglot = s.motor;

                    }
                    _fdb.SaveChanges();
                }

                //其他
                //string[] _mall = { "34", "40", "42", "48", "50", "51", "52" };
                //for (int i = 0; i < _mall.Length; i++)
                //{
                //    spacetxt reseultmall = new spacetxt();
                //    reseultmall = Parking_get_remaintxt(_mall[i]);
                //    if(reseultmall.results != null)
                //    {
                //        int mallId = Convert.ToInt32(reseultmall.results.parking_info[0].code);
                //        ParkinglotInfo _p = _fdb.ParkinglotInfo.Where(o => o.MallId == mallId).FirstOrDefault();
                //        _p.CarParkinglot = reseultmall.results.parking_info[0].car.ToString();
                //        _p.MotocycleParkinglot = reseultmall.results.parking_info[0].motorcycle.ToString();
                //        _fdb.SaveChanges();
                //    }
                //}
            }
            catch(Exception ex)
            {
                _pdb.SystemError_Log.Add(new SystemError_Log
                {
                    Controller = "ParkingRemain",
                    Event = ex.ToString(),
                    ModifyUser = 0,
                    ModifyDate = dt
                });
                _pdb.SaveChanges();
            }

            Close_Winform();
        }

        //關閉Winform
        protected void Close_Winform()
        {
            Environment.Exit(Environment.ExitCode);
            this.Close();
        }

        public SpaceInfo_Result Parking_get_remain()
        {
            SpaceInfo_Result _result = new SpaceInfo_Result();
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://3.114.124.199/ParkingSF_API/api/SpaceInfo");
            request.ContentType = "application/json";
            request.Method = "GET";

            var serializer = new JavaScriptSerializer();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);

            try
            {
                _result = JsonConvert.DeserializeObject<SpaceInfo_Result>(reader.ReadToEnd());
            }
            catch (Exception ex)
            {
                _result.status = -1;
            }
            finally
            {
                reader.Close();
                stream.Close();
                response.Close();
            }
            _result.status = 1;
            return _result;
        }

        public spacetxt Parking_get_remaintxt(string mallId)
        {
            spacetxt _result = new spacetxt();
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://54.65.45.73/upload/parkinginfo/"+ mallId + ".txt");
            request.ContentType = "application/json";
            request.Method = "GET";

            var serializer = new JavaScriptSerializer();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);

            try
            {
                string _read = reader.ReadToEnd() + "}";
                _result = JsonConvert.DeserializeObject<spacetxt>(_read);
                
            }
            catch (Exception ex)
            {
                
            }
            finally
            {
                reader.Close();
                stream.Close();
                response.Close();
            }
            return _result;
        }
    }
}
