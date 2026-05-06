export const Roles = {
  Admin: 'Admin',
  CompanyAdmin: 'CompanyAdmin',
  Agent: 'Agent',
  User: 'User',
} as const

export const Permissions = {
  ManageUsers: 'ManageUsers',
  ManageCompanies: 'ManageCompanies',
  ManageAgents: 'ManageAgents',
  CreateProperty: 'CreateProperty',
  EditProperty: 'EditProperty',
  DeleteProperty: 'DeleteProperty',
  UploadPropertyImages: 'UploadPropertyImages',
  ViewProperties: 'ViewProperties',
  BookViewing: 'BookViewing',
} as const

export const staffRoles = [Roles.Admin, Roles.CompanyAdmin, Roles.Agent]
