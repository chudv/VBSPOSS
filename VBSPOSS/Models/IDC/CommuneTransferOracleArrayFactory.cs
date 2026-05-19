using Oracle.ManagedDataAccess.Types;
using System;

namespace VBSPOSS.Models.IDC
{
    [OracleCustomTypeMapping(
        "VBSPOSS.TAB_COMMUNE_TRANSFER")]
    public class CommuneTransferOracleArrayFactory :
        IOracleArrayTypeFactory
    {
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
    }
}