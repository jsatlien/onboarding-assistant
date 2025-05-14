<template>
  <div class="work-order-create">
    <h2 class="page-title">Create Work Order</h2>
    
    <div class="card">
      <form @submit.prevent="submitWorkOrder">
        <div class="form-grid">
          <!-- Customer Information -->
          <div class="form-section">
            <h3 class="section-title">Customer Information</h3>
            
            <div class="form-group">
              <label for="customer-name" class="form-label">Customer Name</label>
              <input 
                type="text" 
                id="customer-name" 
                class="form-control" 
                v-model="workOrder.customerName" 
                required
              />
            </div>
            
            <div class="form-group">
              <label for="customer-email" class="form-label">Email</label>
              <input 
                type="email" 
                id="customer-email" 
                class="form-control" 
                v-model="workOrder.customerEmail"
              />
            </div>
            
            <div class="form-group">
              <label for="customer-phone" class="form-label">Phone</label>
              <input 
                type="tel" 
                id="customer-phone" 
                class="form-control" 
                v-model="workOrder.customerPhone" 
                required
              />
            </div>
          </div>
          
          <!-- Device Information -->
          <div class="form-section">
            <h3 class="section-title">Device Information</h3>
            
            <div class="form-group">
              <label for="device-type-select" class="form-label">Device Type</label>
              <select 
                id="device-type-select" 
                class="form-control" 
                v-model="workOrder.deviceType" 
                required
              >
                <option value="">Select Device Type</option>
                <option v-for="type in deviceTypes" :key="type" :value="type">
                  {{ type }}
                </option>
              </select>
            </div>
            
            <div class="form-group">
              <label for="device-model" class="form-label">Model</label>
              <input 
                type="text" 
                id="device-model" 
                class="form-control" 
                v-model="workOrder.deviceModel"
              />
            </div>
            
            <div class="form-group">
              <label for="device-serial" class="form-label">Serial Number</label>
              <input 
                type="text" 
                id="device-serial" 
                class="form-control" 
                v-model="workOrder.deviceSerial"
              />
            </div>
          </div>
        </div>
        
        <!-- Repair Information -->
        <div class="form-section">
          <h3 class="section-title">Repair Information</h3>
          
          <div class="form-group">
            <label for="issue-description" class="form-label">Issue Description</label>
            <textarea 
              id="issue-description" 
              class="form-control" 
              rows="4" 
              v-model="workOrder.issueDescription" 
              required
            ></textarea>
          </div>
          
          <div class="form-grid">
            <div class="form-group">
              <label for="priority-select" class="form-label">Priority</label>
              <select 
                id="priority-select" 
                class="form-control" 
                v-model="workOrder.priority" 
                required
              >
                <option value="">Select Priority</option>
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
                <option value="Urgent">Urgent</option>
              </select>
            </div>
            
            <div class="form-group">
              <label for="assign-technician-select" class="form-label">Assign Technician</label>
              <select 
                id="assign-technician-select" 
                class="form-control" 
                v-model="workOrder.technicianId"
              >
                <option value="">Select Technician</option>
                <option v-for="tech in technicians" :key="tech.id" :value="tech.id">
                  {{ tech.name }}
                </option>
              </select>
            </div>
          </div>
        </div>
        
        <!-- Form Actions -->
        <div class="form-actions">
          <button type="button" class="btn btn-secondary" @click="cancel">Cancel</button>
          <button type="submit" class="btn btn-primary" id="submit-button">Create Work Order</button>
        </div>
      </form>
    </div>
  </div>
</template>

<script>
export default {
  name: 'WorkOrderCreate',
  data() {
    return {
      workOrder: {
        customerName: '',
        customerEmail: '',
        customerPhone: '',
        deviceType: '',
        deviceModel: '',
        deviceSerial: '',
        issueDescription: '',
        priority: '',
        technicianId: ''
      },
      deviceTypes: [
        'Laptop',
        'Desktop',
        'Smartphone',
        'Tablet',
        'Printer',
        'Monitor',
        'Other'
      ],
      technicians: [
        { id: 1, name: 'Alex Johnson' },
        { id: 2, name: 'Maria Garcia' },
        { id: 3, name: 'David Kim' },
        { id: 4, name: 'Sarah Williams' }
      ]
    };
  },
  methods: {
    submitWorkOrder() {
      // In a real application, this would make an API call to create the work order
      console.log('Submitting work order:', this.workOrder);
      
      // Show success message
      alert('Work order created successfully!');
      
      // Navigate back to the work orders list
      this.$router.push('/workorders');
    },
    cancel() {
      // Navigate back without saving
      this.$router.push('/workorders');
    }
  }
};
</script>

<style scoped>
.work-order-create {
  max-width: 1000px;
  margin: 0 auto;
}

.page-title {
  margin-bottom: 24px;
  font-size: 24px;
  font-weight: 600;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 24px;
}

.section-title {
  font-size: 16px;
  font-weight: 600;
  margin-bottom: 16px;
  color: #495057;
}

.form-section {
  margin-bottom: 24px;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 16px;
  margin-top: 24px;
  padding-top: 24px;
  border-top: 1px solid var(--border-color);
}

@media (max-width: 768px) {
  .form-grid {
    grid-template-columns: 1fr;
  }
}
</style>
