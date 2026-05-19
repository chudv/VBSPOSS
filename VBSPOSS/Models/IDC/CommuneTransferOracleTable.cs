using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;

namespace VBSPOSS.Models.IDC
{
    [OracleCustomTypeMapping(
        "VBSPOSS.TAB_COMMUNE_TRANSFER")]
    public class CommuneTransferOracleTable :
        IOracleCustomType,
        IOracleArrayTypeFactory
    {
        public CommuneTransferOracleObject[] Items
        {
            get;
            set;
        }

        public Array CreateArray(
            int numElems)
        {
            return new CommuneTransferOracleObject[numElems];
        }

        public Array CreateStatusArray(
            int numElems)
        {
            return null;
        }

        public void FromCustomObject(
            OracleConnection con,
            object pUdt)
        {
            OracleUdt.SetValue(
                con,
                pUdt,
                0,
                Items);
        }

        public void ToCustomObject(
            OracleConnection con,
            object pUdt)
        {
            Items =
                (CommuneTransferOracleObject[])
                OracleUdt.GetValue(
                    con,
                    pUdt,
                    0);
        }
    }
}