using WebAPI.ApiRoutes;

namespace WebAPITests.Unit
{
    public static class AuthorizationData
    {
        public static IEnumerable<object[]> CorrectAuthorizationForAnonymous
        {
            get
            {
                return new[]
                {
                    new object[]{Items.Prefix, Items.GetAllItems, HttpMethod.Get},
                    new object[]{Items.Prefix, Items.GetItemById, HttpMethod.Get},
                    new object[]{Items.Prefix, Items.GetPage, HttpMethod.Get},
                    new object[]{Items.Prefix, Items.GetAllCopies, HttpMethod.Get},
                    new object[]{Items.Prefix, Items.GetAvailableCopies, HttpMethod.Get},

                    new object[]{Accounts.Prefix, Accounts.ConfirmEmail, HttpMethod.Get},
                    new object[]{Accounts.Prefix, Accounts.RefreshToken, HttpMethod.Get},
                    new object[]{Accounts.Prefix, Accounts.Login, HttpMethod.Post},
                    new object[]{Accounts.Prefix, Accounts.ForgotPassword, HttpMethod.Post},
                    new object[]{Accounts.Prefix, Accounts.Register, HttpMethod.Post},
                    new object[]{Accounts.Prefix, Accounts.ResetPassword, HttpMethod.Post},

                    new object[]{Libraries.Prefix, Libraries.GetAllLibraries, HttpMethod.Get },
                    new object[]{Libraries.Prefix, Libraries.GetLibraryById, HttpMethod.Get },
                    new object[]{Libraries.Prefix, Libraries.GetPage, HttpMethod.Get },
                };
            }
        }

        public static IEnumerable<object[]> InCorrectAuthorizationForAnonymous
        {
            get
            {
                return new[]
                {
                    new object[] { Items.Prefix, Items.AddItem, HttpMethod.Post },
                    new object[] { Items.Prefix, Items.AddItems, HttpMethod.Post },
                    new object[] { Items.Prefix, Items.RemoveItem, HttpMethod.Delete },
                    new object[] { Items.Prefix, Items.RemoveItems, HttpMethod.Delete },
                    new object[] { Items.Prefix, Items.UpdateItem, HttpMethod.Put },

                    new object[]{Accounts.Prefix, Accounts.ChangePassword, HttpMethod.Post},
                    new object[]{Accounts.Prefix, Accounts.Logout, HttpMethod.Head},

                    new object[]{Roles.Prefix, Roles.GetPage, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.GetRoleById, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.GetAllRoles, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.GetRolesByUser, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.GetUsersInRole, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.AddRole, HttpMethod.Post},
                    new object[]{Roles.Prefix, Roles.RemoveRole, HttpMethod.Delete},
                    new object[]{Roles.Prefix, Roles.AddUsersToRole, HttpMethod.Post},
                    new object[]{Roles.Prefix, Roles.RemoveRoleFromUsers, HttpMethod.Post},
                    new object[]{Roles.Prefix, Roles.UpdateRole, HttpMethod.Put},

                    new object[]{Users.Prefix, Users.GetUser, HttpMethod.Get},
                    new object[]{Users.Prefix, Users.GetAllUsers, HttpMethod.Get},
                    new object[]{Users.Prefix, Users.GetPage, HttpMethod.Get},
                    new object[]{Users.Prefix, Users.UpdateUser, HttpMethod.Put},
                    new object[]{Users.Prefix, Users.RemoveUser, HttpMethod.Delete},

                    new object[]{Admin.Prefix, Admin.AddAdmin, HttpMethod.Post},
                    new object[]{Admin.Prefix, Admin.AddWorker, HttpMethod.Post},

                     new object[]{Libraries.Prefix, Libraries.UpdateLibrary, HttpMethod.Put },
                    new object[]{Libraries.Prefix, Libraries.RemoveLibrary, HttpMethod.Delete },
                    new object[]{Libraries.Prefix, Libraries.AddLibrary, HttpMethod.Post },

                    new object[]{Copies.Prefix, Copies.AddCopies, HttpMethod.Post },
                    new object[]{Copies.Prefix, Copies.RemoveCopy, HttpMethod.Delete },
                    new object[]{Copies.Prefix, Copies.RemoveCopies, HttpMethod.Delete },
                    new object[]{Copies.Prefix, Copies.GetCopyById, HttpMethod.Get },
                    new object[]{Copies.Prefix, Copies.GetHistoryByInventoryNumber, HttpMethod.Get },
                    new object[]{Copies.Prefix, Copies.GetCurrentRental, HttpMethod.Get },
                    new object[]{Copies.Prefix, Copies.GetCurrentReservation, HttpMethod.Get },
                    new object[]{Copies.Prefix, Copies.IsAvailable, HttpMethod.Get },

                    new object[]{Profiles.Prefix, Profiles.CreateProfile, HttpMethod.Post },
                    new object[]{Profiles.Prefix, Profiles.ActivationProfile, HttpMethod.Patch },
                    new object[]{Profiles.Prefix, Profiles.DeactivationProfile, HttpMethod.Patch },
                    new object[]{Profiles.Prefix, Profiles.GetProfile, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetProfileWithHistory, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetProfileByCardNumber, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetProfileWithHistoryByCardNumber, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetHistoryByCardNumber, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetCurrentRentals, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetCurrentReservations, HttpMethod.Get },
                };


            }
        }

        public static IEnumerable<object[]> CorrectAuthorizationForRoleBasic
        {
            get
            {
                return new[]
                {
                    new object[]{Items.Prefix, Items.GetAllItems, HttpMethod.Get},
                    new object[]{Items.Prefix, Items.GetItemById, HttpMethod.Get},
                    new object[]{Items.Prefix, Items.GetPage, HttpMethod.Get},
                    new object[]{Items.Prefix, Items.GetAllCopies, HttpMethod.Get},
                    new object[]{Items.Prefix, Items.GetAvailableCopies, HttpMethod.Get},

                    new object[]{Accounts.Prefix, Accounts.ConfirmEmail, HttpMethod.Get},
                    new object[]{Accounts.Prefix, Accounts.RefreshToken, HttpMethod.Get},
                    new object[]{Accounts.Prefix, Accounts.Login, HttpMethod.Post},
                    new object[]{Accounts.Prefix, Accounts.ChangePassword, HttpMethod.Post},
                    new object[]{Accounts.Prefix, Accounts.ForgotPassword, HttpMethod.Post},
                    new object[]{Accounts.Prefix, Accounts.Register, HttpMethod.Post},
                    new object[]{Accounts.Prefix, Accounts.ResetPassword, HttpMethod.Post},
                    //new object[]{Accounts.Prefix, Accounts.Logout, HttpMethod.Head},

                    new object[]{Copies.Prefix, Copies.GetCopyById, HttpMethod.Get },
                    new object[]{Copies.Prefix, Copies.IsAvailable, HttpMethod.Get },

                    new object[]{Users.Prefix, Users.GetUser, HttpMethod.Get},

                    new object[]{Libraries.Prefix, Libraries.GetAllLibraries, HttpMethod.Get },
                    new object[]{Libraries.Prefix, Libraries.GetLibraryById, HttpMethod.Get },
                    new object[]{Libraries.Prefix, Libraries.GetPage, HttpMethod.Get },

                    new object[]{Profiles.Prefix, Profiles.CreateProfile, HttpMethod.Post },      
                    new object[]{Profiles.Prefix, Profiles.GetProfile, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetProfileWithHistory, HttpMethod.Get },
                    
                };
            }
        }

        public static IEnumerable<object[]> InCorrectAuthorizationForRoleBasic
        {
            get
            {
                return new[]
                {
                    new object[]{Items.Prefix, Items.AddItem, HttpMethod.Post},
                    new object[]{Items.Prefix, Items.AddItems, HttpMethod.Post},
                    new object[]{Items.Prefix, Items.RemoveItem, HttpMethod.Delete},
                    new object[]{Items.Prefix, Items.RemoveItems, HttpMethod.Delete},
                    new object[]{Items.Prefix, Items.UpdateItem, HttpMethod.Put},

                    new object[]{Admin.Prefix, Admin.AddAdmin, HttpMethod.Post},
                    new object[]{Admin.Prefix, Admin.AddWorker, HttpMethod.Post},

                    new object[]{Roles.Prefix, Roles.GetPage, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.GetRoleById, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.GetAllRoles, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.GetRolesByUser, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.GetUsersInRole, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.AddRole, HttpMethod.Post},
                    new object[]{Roles.Prefix, Roles.RemoveRole, HttpMethod.Delete},
                    new object[]{Roles.Prefix, Roles.AddUsersToRole, HttpMethod.Post},
                    new object[]{Roles.Prefix, Roles.RemoveRoleFromUsers, HttpMethod.Post},
                    new object[]{Roles.Prefix, Roles.UpdateRole, HttpMethod.Put},

                    new object[]{Users.Prefix, Users.GetAllUsers, HttpMethod.Get},
                    new object[]{Users.Prefix, Users.GetPage, HttpMethod.Get},
                    new object[]{Users.Prefix, Users.UpdateUser, HttpMethod.Put},
                    new object[]{Users.Prefix, Users.RemoveUser, HttpMethod.Delete},

                    new object[]{Libraries.Prefix, Libraries.UpdateLibrary, HttpMethod.Put },
                    new object[]{Libraries.Prefix, Libraries.RemoveLibrary, HttpMethod.Delete },
                    new object[]{Libraries.Prefix, Libraries.AddLibrary, HttpMethod.Post },

                    new object[]{Copies.Prefix, Copies.AddCopies, HttpMethod.Post },
                    new object[]{Copies.Prefix, Copies.RemoveCopy, HttpMethod.Delete },
                    new object[]{Copies.Prefix, Copies.RemoveCopies, HttpMethod.Delete },
                    new object[]{Copies.Prefix, Copies.GetHistoryByInventoryNumber, HttpMethod.Get },
                    new object[]{Copies.Prefix, Copies.GetCurrentRental, HttpMethod.Get },
                    new object[]{Copies.Prefix, Copies.GetCurrentReservation, HttpMethod.Get },

                    new object[]{Profiles.Prefix, Profiles.ActivationProfile, HttpMethod.Patch },
                    new object[]{Profiles.Prefix, Profiles.DeactivationProfile, HttpMethod.Patch },
                    new object[]{Profiles.Prefix, Profiles.GetProfileByCardNumber, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetProfileWithHistoryByCardNumber, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetHistoryByCardNumber, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetCurrentRentals, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetCurrentReservations, HttpMethod.Get },
                };
            }
        }

        public static IEnumerable<object[]> CorrectAuthorizationForRoleWorker
        {
            get
            {
                return new[]
                {
                    new object[]{Items.Prefix, Items.GetAllItems, HttpMethod.Get},
                    new object[]{Items.Prefix, Items.GetItemById, HttpMethod.Get},
                    new object[]{Items.Prefix, Items.GetPage, HttpMethod.Get},
                    new object[]{Items.Prefix, Items.AddItem, HttpMethod.Post},
                    new object[]{Items.Prefix, Items.AddItems, HttpMethod.Post},
                    new object[]{Items.Prefix, Items.RemoveItem, HttpMethod.Delete},
                    new object[]{Items.Prefix, Items.RemoveItems, HttpMethod.Delete},
                    new object[]{Items.Prefix, Items.UpdateItem, HttpMethod.Put},
                    new object[]{Items.Prefix, Items.GetAllCopies, HttpMethod.Get},
                    new object[]{Items.Prefix, Items.GetAvailableCopies, HttpMethod.Get},

                    new object[]{Accounts.Prefix, Accounts.ConfirmEmail, HttpMethod.Get},
                    new object[]{Accounts.Prefix, Accounts.RefreshToken, HttpMethod.Get},
                    new object[]{Accounts.Prefix, Accounts.Login, HttpMethod.Post},
                    new object[]{Accounts.Prefix, Accounts.ChangePassword, HttpMethod.Post},
                    new object[]{Accounts.Prefix, Accounts.ForgotPassword, HttpMethod.Post},
                    new object[]{Accounts.Prefix, Accounts.Register, HttpMethod.Post},
                    new object[]{Accounts.Prefix, Accounts.ResetPassword, HttpMethod.Post},
                    //new object[]{Accounts.Prefix, Accounts.Logout, HttpMethod.Head},

                    new object[]{Users.Prefix, Users.GetUser, HttpMethod.Get},

                    new object[]{Libraries.Prefix, Libraries.GetAllLibraries, HttpMethod.Get },
                    new object[]{Libraries.Prefix, Libraries.GetLibraryById, HttpMethod.Get },
                    new object[]{Libraries.Prefix, Libraries.GetPage, HttpMethod.Get },

                    new object[]{Copies.Prefix, Copies.AddCopies, HttpMethod.Post },
                    new object[]{Copies.Prefix, Copies.RemoveCopy, HttpMethod.Delete },
                    new object[]{Copies.Prefix, Copies.RemoveCopies, HttpMethod.Delete },
                    new object[]{Copies.Prefix, Copies.GetCopyById, HttpMethod.Get },
                    new object[]{Copies.Prefix, Copies.GetHistoryByInventoryNumber, HttpMethod.Get },
                    new object[]{Copies.Prefix, Copies.GetCurrentRental, HttpMethod.Get },
                    new object[]{Copies.Prefix, Copies.GetCurrentReservation, HttpMethod.Get },
                    new object[]{Copies.Prefix, Copies.IsAvailable, HttpMethod.Get },

                    new object[]{Profiles.Prefix, Profiles.CreateProfile, HttpMethod.Post },
                    new object[]{Profiles.Prefix, Profiles.ActivationProfile, HttpMethod.Patch },
                    new object[]{Profiles.Prefix, Profiles.DeactivationProfile, HttpMethod.Patch },
                    new object[]{Profiles.Prefix, Profiles.GetProfile, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetProfileWithHistory, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetProfileByCardNumber, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetProfileWithHistoryByCardNumber, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetHistoryByCardNumber, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetCurrentRentals, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetCurrentReservations, HttpMethod.Get },
                };
            }
        }

        public static IEnumerable<object[]> InCorrectAuthorizationForRoleWorker
        {
            get
            {
                return new[]
                {
                    new object[]{Admin.Prefix, Admin.AddAdmin, HttpMethod.Post},
                    new object[]{Admin.Prefix, Admin.AddWorker, HttpMethod.Post},

                    new object[]{Roles.Prefix, Roles.GetPage, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.GetRoleById, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.GetAllRoles, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.GetRolesByUser, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.GetUsersInRole, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.AddRole, HttpMethod.Post},
                    new object[]{Roles.Prefix, Roles.RemoveRole, HttpMethod.Delete},
                    new object[]{Roles.Prefix, Roles.AddUsersToRole, HttpMethod.Post},
                    new object[]{Roles.Prefix, Roles.RemoveRoleFromUsers, HttpMethod.Post},
                    new object[]{Roles.Prefix, Roles.UpdateRole, HttpMethod.Put},

                    new object[]{Users.Prefix, Users.GetAllUsers, HttpMethod.Get},
                    new object[]{Users.Prefix, Users.GetPage, HttpMethod.Get},
                    new object[]{Users.Prefix, Users.UpdateUser, HttpMethod.Put},
                    new object[]{Users.Prefix, Users.RemoveUser, HttpMethod.Delete},

                    new object[]{Libraries.Prefix, Libraries.UpdateLibrary, HttpMethod.Put },
                    new object[]{Libraries.Prefix, Libraries.RemoveLibrary, HttpMethod.Delete },
                    new object[]{Libraries.Prefix, Libraries.AddLibrary, HttpMethod.Post },
                };
            }
        }

        public static IEnumerable<object[]> CorrectAuthorizationForRoleModerator
        {
            get
            {
                return new[]
                {
                    new object[]{Items.Prefix, Items.GetAllItems, HttpMethod.Get},
                    new object[]{Items.Prefix, Items.GetItemById, HttpMethod.Get},
                    new object[]{Items.Prefix, Items.GetPage, HttpMethod.Get},
                    new object[]{Items.Prefix, Items.AddItem, HttpMethod.Post},
                    new object[]{Items.Prefix, Items.AddItems, HttpMethod.Post},
                    new object[]{Items.Prefix, Items.RemoveItem, HttpMethod.Delete},
                    new object[]{Items.Prefix, Items.RemoveItems, HttpMethod.Delete},
                    new object[]{Items.Prefix, Items.UpdateItem, HttpMethod.Put},
                    new object[]{Items.Prefix, Items.GetAllCopies, HttpMethod.Get},
                    new object[]{Items.Prefix, Items.GetAvailableCopies, HttpMethod.Get},

                    new object[]{Accounts.Prefix, Accounts.ConfirmEmail, HttpMethod.Get},
                    new object[]{Accounts.Prefix, Accounts.RefreshToken, HttpMethod.Get},
                    new object[]{Accounts.Prefix, Accounts.Login, HttpMethod.Post},
                    new object[]{Accounts.Prefix, Accounts.ChangePassword, HttpMethod.Post},
                    new object[]{Accounts.Prefix, Accounts.ForgotPassword, HttpMethod.Post},
                    new object[]{Accounts.Prefix, Accounts.Register, HttpMethod.Post},
                    new object[]{Accounts.Prefix, Accounts.ResetPassword, HttpMethod.Post},
                    //new object[]{Accounts.Prefix, Accounts.Logout, HttpMethod.Head},

                    new object[]{Roles.Prefix, Roles.GetPage, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.GetRoleById, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.GetAllRoles, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.GetRolesByUser, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.GetUsersInRole, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.AddRole, HttpMethod.Post},
                    new object[]{Roles.Prefix, Roles.RemoveRole, HttpMethod.Delete},
                    new object[]{Roles.Prefix, Roles.AddUsersToRole, HttpMethod.Post},
                    new object[]{Roles.Prefix, Roles.RemoveRoleFromUsers, HttpMethod.Post},
                    new object[]{Roles.Prefix, Roles.UpdateRole, HttpMethod.Put},

                    new object[]{Users.Prefix, Users.GetUser, HttpMethod.Get},
                    new object[]{Users.Prefix, Users.GetAllUsers, HttpMethod.Get},
                    new object[]{Users.Prefix, Users.GetPage, HttpMethod.Get},

                      new object[]{Libraries.Prefix, Libraries.GetAllLibraries, HttpMethod.Get },
                    new object[]{Libraries.Prefix, Libraries.GetLibraryById, HttpMethod.Get },
                    new object[]{Libraries.Prefix, Libraries.GetPage, HttpMethod.Get },
                    new object[]{Libraries.Prefix, Libraries.UpdateLibrary, HttpMethod.Put },
                    new object[]{Libraries.Prefix, Libraries.RemoveLibrary, HttpMethod.Delete },
                    new object[]{Libraries.Prefix, Libraries.AddLibrary, HttpMethod.Post },

                    new object[]{Copies.Prefix, Copies.AddCopies, HttpMethod.Post },
                    new object[]{Copies.Prefix, Copies.RemoveCopy, HttpMethod.Delete },
                    new object[]{Copies.Prefix, Copies.RemoveCopies, HttpMethod.Delete },
                    new object[]{Copies.Prefix, Copies.GetCopyById, HttpMethod.Get },
                    new object[]{Copies.Prefix, Copies.GetHistoryByInventoryNumber, HttpMethod.Get },
                    new object[]{Copies.Prefix, Copies.GetCurrentRental, HttpMethod.Get },
                    new object[]{Copies.Prefix, Copies.GetCurrentReservation, HttpMethod.Get },
                    new object[]{Copies.Prefix, Copies.IsAvailable, HttpMethod.Get },

                    new object[]{Profiles.Prefix, Profiles.CreateProfile, HttpMethod.Post },
                    new object[]{Profiles.Prefix, Profiles.ActivationProfile, HttpMethod.Patch },
                    new object[]{Profiles.Prefix, Profiles.DeactivationProfile, HttpMethod.Patch },
                    new object[]{Profiles.Prefix, Profiles.GetProfile, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetProfileWithHistory, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetProfileByCardNumber, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetProfileWithHistoryByCardNumber, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetHistoryByCardNumber, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetCurrentRentals, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetCurrentReservations, HttpMethod.Get },
                };
            }
        }

        public static IEnumerable<object[]> InCorrectAuthorizationForRoleModerator
        {
            get
            {
                return new[]
                {
                    new object[]{Admin.Prefix, Admin.AddAdmin, HttpMethod.Post},
                    new object[]{Admin.Prefix, Admin.AddWorker, HttpMethod.Post},

                    new object[]{Users.Prefix, Users.UpdateUser, HttpMethod.Put},
                    new object[]{Users.Prefix, Users.RemoveUser, HttpMethod.Delete},
                };
            }
        }

        public static IEnumerable<object[]> CorrectAuthorizationForRoleAdmin
        {
            get
            {
                return new[]
                {
                    new object[]{Items.Prefix, Items.GetAllItems, HttpMethod.Get},
                    new object[]{Items.Prefix, Items.GetItemById, HttpMethod.Get},
                    new object[]{Items.Prefix, Items.GetPage, HttpMethod.Get},
                    new object[]{Items.Prefix, Items.AddItem, HttpMethod.Post},
                    new object[]{Items.Prefix, Items.AddItems, HttpMethod.Post},
                    new object[]{Items.Prefix, Items.RemoveItem, HttpMethod.Delete},
                    new object[]{Items.Prefix, Items.RemoveItems, HttpMethod.Delete},
                    new object[]{Items.Prefix, Items.UpdateItem, HttpMethod.Put},
                    new object[]{Items.Prefix, Items.GetAllCopies, HttpMethod.Get},
                    new object[]{Items.Prefix, Items.GetAvailableCopies, HttpMethod.Get},

                    new object[]{Accounts.Prefix, Accounts.ConfirmEmail, HttpMethod.Get},
                    new object[]{Accounts.Prefix, Accounts.RefreshToken, HttpMethod.Get},
                    new object[]{Accounts.Prefix, Accounts.Login, HttpMethod.Post},
                    new object[]{Accounts.Prefix, Accounts.ChangePassword, HttpMethod.Post},
                    new object[]{Accounts.Prefix, Accounts.ForgotPassword, HttpMethod.Post},
                    new object[]{Accounts.Prefix, Accounts.Register, HttpMethod.Post},
                    new object[]{Accounts.Prefix, Accounts.ResetPassword, HttpMethod.Post},
                    //new object[]{Accounts.Prefix, Accounts.Logout, HttpMethod.Head},

                    new object[]{Roles.Prefix, Roles.GetPage, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.GetRoleById, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.GetAllRoles, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.GetRolesByUser, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.GetUsersInRole, HttpMethod.Get},
                    new object[]{Roles.Prefix, Roles.AddRole, HttpMethod.Post},
                    new object[]{Roles.Prefix, Roles.RemoveRole, HttpMethod.Delete},
                    new object[]{Roles.Prefix, Roles.AddUsersToRole, HttpMethod.Post},
                    new object[]{Roles.Prefix, Roles.RemoveRoleFromUsers, HttpMethod.Post},
                    new object[]{Roles.Prefix, Roles.UpdateRole, HttpMethod.Put},

                    new object[]{Users.Prefix, Users.GetUser, HttpMethod.Get},
                    new object[]{Users.Prefix, Users.GetAllUsers, HttpMethod.Get},
                    new object[]{Users.Prefix, Users.GetPage, HttpMethod.Get},
                    new object[]{Users.Prefix, Users.UpdateUser, HttpMethod.Put},
                    new object[]{Users.Prefix, Users.RemoveUser, HttpMethod.Delete},

                    new object[]{Admin.Prefix, Admin.AddAdmin, HttpMethod.Post},
                    new object[]{Admin.Prefix, Admin.AddWorker, HttpMethod.Post},

                    new object[]{Libraries.Prefix, Libraries.GetAllLibraries, HttpMethod.Get },
                    new object[]{Libraries.Prefix, Libraries.GetLibraryById, HttpMethod.Get },
                    new object[]{Libraries.Prefix, Libraries.GetPage, HttpMethod.Get },
                    new object[]{Libraries.Prefix, Libraries.UpdateLibrary, HttpMethod.Put },
                    new object[]{Libraries.Prefix, Libraries.RemoveLibrary, HttpMethod.Delete },
                    new object[]{Libraries.Prefix, Libraries.AddLibrary, HttpMethod.Post },

                    new object[]{Copies.Prefix, Copies.AddCopies, HttpMethod.Post },
                    new object[]{Copies.Prefix, Copies.RemoveCopy, HttpMethod.Delete },
                    new object[]{Copies.Prefix, Copies.RemoveCopies, HttpMethod.Delete },
                    new object[]{Copies.Prefix, Copies.GetCopyById, HttpMethod.Get },
                    new object[]{Copies.Prefix, Copies.GetHistoryByInventoryNumber, HttpMethod.Get },
                    new object[]{Copies.Prefix, Copies.GetCurrentRental, HttpMethod.Get },
                    new object[]{Copies.Prefix, Copies.GetCurrentReservation, HttpMethod.Get },
                    new object[]{Copies.Prefix, Copies.IsAvailable, HttpMethod.Get },

                    new object[]{Profiles.Prefix, Profiles.CreateProfile, HttpMethod.Post },
                    new object[]{Profiles.Prefix, Profiles.ActivationProfile, HttpMethod.Patch },
                    new object[]{Profiles.Prefix, Profiles.DeactivationProfile, HttpMethod.Patch },
                    new object[]{Profiles.Prefix, Profiles.GetProfile, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetProfileWithHistory, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetProfileByCardNumber, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetProfileWithHistoryByCardNumber, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetHistoryByCardNumber, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetCurrentRentals, HttpMethod.Get },
                    new object[]{Profiles.Prefix, Profiles.GetCurrentReservations, HttpMethod.Get },
                };
            }
        }

        public static IEnumerable<object[]> InCorrectAuthorizationForRoleAdmin
        {
            get
            {
                return null;
            }
        }
    }
}