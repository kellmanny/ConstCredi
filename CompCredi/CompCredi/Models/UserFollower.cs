namespace CompCredi.Models {
    public class UserFollower {
        public int Id { get; set; }
        public int FollowerId { get; set; } // Usuário que está seguindo
        public int FollowingId { get; set; } // Usuário que está sendo seguido

        public User Follower { get; set; }
        public User Following { get; set; }
    }
}
