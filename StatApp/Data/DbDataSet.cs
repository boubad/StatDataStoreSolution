//------------------------------------------------------------------------------
// <auto-generated>
//    Ce code a été généré à partir d'un modèle.
//
//    Des modifications manuelles apportées à ce fichier peuvent conduire à un comportement inattendu de votre application.
//    Les modifications manuelles apportées à ce fichier sont remplacées si le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StatApp.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class DbDataSet
    {
        public DbDataSet()
        {
            this.Variables = new HashSet<DbVariable>();
        }
    
        public int Id { get; set; }
        public int LastIndex { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    
        public virtual ICollection<DbVariable> Variables { get; set; }
    }
}
