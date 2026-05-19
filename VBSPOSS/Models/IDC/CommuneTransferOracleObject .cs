using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;

namespace VBSPOSS.Models.IDC
{
    [OracleCustomTypeMapping(
        "VBSPOSS.OBJ_COMMUNE_TRANSFER")]
    public class CommuneTransferOracleObject :
        IOracleCustomType,
        INullable
    {
        [OracleObjectMapping("PROV_CD")]
        public string PROV_CD { get; set; }

        [OracleObjectMapping("DIST_CD")]
        public string DIST_CD { get; set; }

        [OracleObjectMapping("COMM_CD")]
        public string COMM_CD { get; set; }

        [OracleObjectMapping("SCOM_CD")]
        public string SCOM_CD { get; set; }

        [OracleObjectMapping("COMM_NAME")]
        public string COMM_NAME { get; set; }

        [OracleObjectMapping("TXN_ID")]
        public string TXN_ID { get; set; }

        [OracleObjectMapping("VISIT_DT")]
        public string VISIT_DT { get; set; }

        [OracleObjectMapping("POS_CD")]
        public string POS_CD { get; set; }

        [OracleObjectMapping("NEW_COMMUNE")]
        public string NEW_COMMUNE { get; set; }

        [OracleObjectMapping("FEE_LEN_AREA")]
        public string FEE_LEN_AREA { get; set; }

        [OracleObjectMapping("COM_LEN_AREA")]
        public string COM_LEN_AREA { get; set; }

        [OracleObjectMapping("CASA_AREA")]
        public string CASA_AREA { get; set; }

        [OracleObjectMapping("TYPE_CD")]
        public string TYPE_CD { get; set; }

        [OracleObjectMapping("JOB_ID")]
        public decimal JOB_ID { get; set; }

        [OracleObjectMapping("NEW_POS_CD")]
        public string NEW_POS_CD { get; set; }

        [OracleObjectMapping("DIFF_AREA")]
        public string DIFF_AREA { get; set; }

        public bool IsNull => false;

        public void FromCustomObject(
            OracleConnection con,
            object pUdt)
        {
            OracleUdt.SetValue(con, pUdt, "PROV_CD", PROV_CD);
            OracleUdt.SetValue(con, pUdt, "DIST_CD", DIST_CD);
            OracleUdt.SetValue(con, pUdt, "COMM_CD", COMM_CD);
            OracleUdt.SetValue(con, pUdt, "SCOM_CD", SCOM_CD);
            OracleUdt.SetValue(con, pUdt, "COMM_NAME", COMM_NAME);
            OracleUdt.SetValue(con, pUdt, "TXN_ID", TXN_ID);
            OracleUdt.SetValue(con, pUdt, "VISIT_DT", VISIT_DT);
            OracleUdt.SetValue(con, pUdt, "POS_CD", POS_CD);
            OracleUdt.SetValue(con, pUdt, "NEW_COMMUNE", NEW_COMMUNE);
            OracleUdt.SetValue(con, pUdt, "FEE_LEN_AREA", FEE_LEN_AREA);
            OracleUdt.SetValue(con, pUdt, "COM_LEN_AREA", COM_LEN_AREA);
            OracleUdt.SetValue(con, pUdt, "CASA_AREA", CASA_AREA);
            OracleUdt.SetValue(con, pUdt, "TYPE_CD", TYPE_CD);
            OracleUdt.SetValue(con, pUdt, "JOB_ID", JOB_ID);
            OracleUdt.SetValue(con, pUdt, "NEW_POS_CD", NEW_POS_CD);
            OracleUdt.SetValue(con, pUdt, "DIFF_AREA", DIFF_AREA);
        }

        public void ToCustomObject(
            OracleConnection con,
            object pUdt)
        {
        }
    }
}