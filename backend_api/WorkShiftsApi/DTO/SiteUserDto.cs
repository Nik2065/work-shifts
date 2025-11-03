namespace WorkShiftsApi.DTO
{
    public class SiteUserDto
    {
        public int Id { get; set; }
        public string Login { get; set; }//email
        public DateTime Created { get; set; }

        //public bool Deleted {}
        public string RoleName { get; set; }
        public string RoleCode { get; set; }

        public List<ObjectDb> ObjectsList { get; set; }
        public List<int> ObjectsListIds { get; set; }
    }
}
