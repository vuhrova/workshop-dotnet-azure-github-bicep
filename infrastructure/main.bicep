@allowed(['dev', 'prod'])
param environment string

targetScope = 'resourceGroup'

module appService 'appservice.bicep' = {
  name: 'appservice'
  params: {
    appName: 'dnazghbicep'
    myName: 'vuhrova'
    location: 'centralus'
    environment: environment
  }
}
