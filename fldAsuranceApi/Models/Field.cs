//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace fldAsuranceApi.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Field
    {
        public int fldid { get; set; }
        public string Lang { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public string CName { get; set; }
        public string CTitle { get; set; }
        public string FacName { get; set; }
        public string FacAddress { get; set; }
        public string FacPhone { get; set; }
        public string SurgeonName { get; set; }
        public string SurgeonEmail { get; set; }
        public string CryolifeRep { get; set; }
        public string RegionMgr { get; set; }
        public string LetterAck { get; set; }
        public string LetterFinal { get; set; }
        public string LetterNone { get; set; }
        public string LetterSendTo { get; set; }
        public string ReturnSample { get; set; }
        public string RMANumber { get; set; }
        public string Product { get; set; }
        public string SerialLotNumber { get; set; }
        public string UDI { get; set; }
        public string Implanted { get; set; }
        public Nullable<System.DateTime> DateIncident { get; set; }
        public Nullable<System.DateTime> DateReported { get; set; }
        public string Outcome { get; set; }
        public string Description { get; set; }
        public string DocumentNumber { get; set; }
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        public string SubmittedBy { get; set; }
        public string SubmittedByEmail { get; set; }
    }
}