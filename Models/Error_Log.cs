//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DJLocal.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Error_Log
    {
        public long Id { get; set; }
        public string Accountno { get; set; }
        public string Requstertin { get; set; }
        public string Mandant_Name { get; set; }
        public Nullable<long> Mandant_Id { get; set; }
        public string Error_Messages { get; set; }
        public string Doc_Type { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<System.DateTime> Date_Created { get; set; }
        public string UserID { get; set; }
        public Nullable<System.DateTime> TimeStamp { get; set; }
    }
}
