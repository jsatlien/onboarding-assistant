<template>
  <div class="repair-programs">
    <div class="page-header">
      <h2 class="page-title">Repair Programs</h2>
      <button class="btn btn-primary">
        <span class="btn-icon">+</span> New Program
      </button>
    </div>
    
    <div class="programs-grid">
      <div class="card program-card" v-for="program in repairPrograms" :key="program.id">
        <div class="program-header">
          <h3 class="program-title">{{ program.name }}</h3>
          <span class="program-badge" :class="program.active ? 'badge-active' : 'badge-inactive'">
            {{ program.active ? 'Active' : 'Inactive' }}
          </span>
        </div>
        
        <div class="program-details">
          <div class="detail-item">
            <span class="detail-label">Device Types:</span>
            <span class="detail-value">{{ program.deviceTypes.join(', ') }}</span>
          </div>
          
          <div class="detail-item">
            <span class="detail-label">Avg. Completion Time:</span>
            <span class="detail-value">{{ program.avgCompletionTime }}</span>
          </div>
          
          <div class="detail-item">
            <span class="detail-label">Workflows:</span>
            <span class="detail-value">{{ program.workflows.length }}</span>
          </div>
        </div>
        
        <div class="program-description">
          {{ program.description }}
        </div>
        
        <div class="workflow-list">
          <h4 class="workflow-title">Workflows</h4>
          <ul class="workflow-items">
            <li v-for="(workflow, index) in program.workflows" :key="index">
              {{ workflow }}
            </li>
          </ul>
        </div>
        
        <div class="program-actions">
          <button class="btn btn-secondary">Edit</button>
          <button class="btn btn-secondary">View Details</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'RepairPrograms',
  data() {
    return {
      repairPrograms: [
        {
          id: 1,
          name: 'Standard Laptop Repair',
          active: true,
          deviceTypes: ['Laptop'],
          avgCompletionTime: '3-5 days',
          description: 'Standard repair program for laptops covering hardware diagnostics, software issues, and basic component replacement.',
          workflows: [
            'Initial Assessment',
            'Hardware Diagnostics',
            'Software Troubleshooting',
            'Component Replacement',
            'Quality Assurance',
            'Customer Delivery'
          ]
        },
        {
          id: 2,
          name: 'Premium Phone Service',
          active: true,
          deviceTypes: ['Smartphone'],
          avgCompletionTime: '1-2 days',
          description: 'Expedited repair service for smartphones with priority handling and same-day screen replacements when possible.',
          workflows: [
            'Express Intake',
            'Diagnostics',
            'Repair',
            'Express Quality Check',
            'Customer Notification'
          ]
        },
        {
          id: 3,
          name: 'Desktop Overhaul',
          active: true,
          deviceTypes: ['Desktop'],
          avgCompletionTime: '5-7 days',
          description: 'Comprehensive desktop computer repair and upgrade service, including performance optimization and component upgrades.',
          workflows: [
            'System Evaluation',
            'Hardware Testing',
            'Component Upgrade',
            'System Optimization',
            'Benchmark Testing',
            'Customer Delivery'
          ]
        },
        {
          id: 4,
          name: 'Tablet Quick Fix',
          active: true,
          deviceTypes: ['Tablet'],
          avgCompletionTime: '2-3 days',
          description: 'Specialized tablet repair program focusing on screen replacements, battery issues, and charging problems.',
          workflows: [
            'Initial Inspection',
            'Diagnostics',
            'Parts Ordering',
            'Repair',
            'Testing',
            'Customer Pickup'
          ]
        },
        {
          id: 5,
          name: 'Legacy Device Support',
          active: false,
          deviceTypes: ['Laptop', 'Desktop', 'Other'],
          avgCompletionTime: '7-10 days',
          description: 'Specialized program for older devices that require legacy parts or software. Limited availability.',
          workflows: [
            'Compatibility Assessment',
            'Parts Sourcing',
            'Repair Planning',
            'Restoration',
            'Software Configuration',
            'Final Testing'
          ]
        },
        {
          id: 6,
          name: 'Business Continuity',
          active: true,
          deviceTypes: ['Laptop', 'Desktop', 'Printer', 'Server'],
          avgCompletionTime: '1-3 days',
          description: 'Priority service for business clients with loaner devices and after-hours support options.',
          workflows: [
            'Business Impact Assessment',
            'Loaner Setup',
            'Expedited Repair',
            'Data Migration',
            'Business Continuity Check',
            'Client Handoff'
          ]
        }
      ]
    };
  }
};
</script>

<style scoped>
.repair-programs {
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.page-title {
  font-size: 24px;
  font-weight: 600;
  margin: 0;
}

.btn-icon {
  margin-right: 8px;
}

.programs-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 24px;
}

.program-card {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.program-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.program-title {
  font-size: 18px;
  font-weight: 600;
  margin: 0;
}

.program-badge {
  display: inline-block;
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
}

.badge-active {
  background-color: #d1e7dd;
  color: #0f5132;
}

.badge-inactive {
  background-color: #f8d7da;
  color: #842029;
}

.program-details {
  margin-bottom: 16px;
}

.detail-item {
  margin-bottom: 8px;
}

.detail-label {
  font-weight: 500;
  margin-right: 8px;
  color: #6c757d;
}

.program-description {
  margin-bottom: 16px;
  color: #495057;
  line-height: 1.5;
}

.workflow-title {
  font-size: 16px;
  font-weight: 600;
  margin-bottom: 12px;
  color: #495057;
}

.workflow-items {
  list-style-type: none;
  padding-left: 0;
  margin-bottom: 16px;
}

.workflow-items li {
  padding: 6px 0;
  border-bottom: 1px solid var(--border-color);
  color: #495057;
}

.workflow-items li:last-child {
  border-bottom: none;
}

.program-actions {
  margin-top: auto;
  display: flex;
  gap: 12px;
}
</style>
