targetScope = 'subscription'

param budgetName string = 'homeservice-monthly-20'
param contactEmail string
param startDate string
param endDate string
param amount int = 15

resource budget 'Microsoft.Consumption/budgets@2023-11-01' = {
  name: budgetName
  properties: {
    category: 'Cost'
    amount: amount
    timeGrain: 'Monthly'
    timePeriod: {
      startDate: startDate
      endDate: endDate
    }
    notifications: {
      Actual50: {
        enabled: true
        operator: 'GreaterThanOrEqualTo'
        threshold: 50
        thresholdType: 'Actual'
        contactEmails: [ contactEmail ]
        contactGroups: []
        contactRoles: []
      }
      Actual75: {
        enabled: true
        operator: 'GreaterThanOrEqualTo'
        threshold: 75
        thresholdType: 'Actual'
        contactEmails: [ contactEmail ]
        contactGroups: []
        contactRoles: []
      }
      Actual90: {
        enabled: true
        operator: 'GreaterThanOrEqualTo'
        threshold: 90
        thresholdType: 'Actual'
        contactEmails: [ contactEmail ]
        contactGroups: []
        contactRoles: []
      }
      Actual100: {
        enabled: true
        operator: 'GreaterThanOrEqualTo'
        threshold: 100
        thresholdType: 'Actual'
        contactEmails: [ contactEmail ]
        contactGroups: []
        contactRoles: []
      }
      Forecasted75: {
        enabled: true
        operator: 'GreaterThanOrEqualTo'
        threshold: 75
        thresholdType: 'Forecasted'
        contactEmails: [ contactEmail ]
        contactGroups: []
        contactRoles: []
      }
    }
  }
}

output monthlyBudget int = amount
