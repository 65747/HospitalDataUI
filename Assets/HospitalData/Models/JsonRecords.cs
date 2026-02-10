using System;
using System.Collections.Generic;

namespace Hospital.Data.Models
{
// Structures alignées sur les fichiers JSON dans Data/
// Noms de propriétés volontairement alignés sur les clés JSON pour sérialisation sans attributs.
public class PatientJson
{
    public string IDpatient { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public int date_de_naissance { get; set; }
    public string Sexe { get; set; } = string.Empty;
    public string Pathologie { get; set; } = string.Empty;
    public string CoteNeglige { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public string SuiviPatient { get; set; } = string.Empty;
}

public class SuperviseurJson
{
    public string IdSuperviseur { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string fonction { get; set; } = string.Empty;
}

public class SessionJson
{
    public string IDpatient { get; set; } = string.Empty;
    public string EnvironnementUtilise { get; set; } = string.Empty;
    public string PositionDepart { get; set; } = string.Empty;
    public DateTime DateDebut { get; set; }
    public string niveauDifficulte { get; set; } = string.Empty;
    public int NiveauAssistance_moyen { get; set; }
    public string ObjectifsAtteints { get; set; } = string.Empty;
    public string ObjectifsManques { get; set; } = string.Empty;
    public int duree { get; set; }
    public int ScoreTotal { get; set; }
    public string IdSuperviseur { get; set; } = string.Empty;
    public double TempsReaction { get; set; }
    public double PrecisionPointage { get; set; }
    public string Commentaire { get; set; } = string.Empty;
}

public class SessionEnvelope
{
    public List<SessionJson> Sessions { get; set; } = new();
}
}
