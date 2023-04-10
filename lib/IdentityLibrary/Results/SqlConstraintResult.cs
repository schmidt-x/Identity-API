namespace IdentityLibrary.Results;

public class SqlConstraintResult
{
	public string Constraint { get; set; }
	public string Table { get; set; }
	public string Column { get; set; }
	public string ErrorMessage { get; set; }
}