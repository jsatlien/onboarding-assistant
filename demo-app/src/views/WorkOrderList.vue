<template>
  <div class="work-order-list">
    <div class="page-header">
      <h2 class="page-title">Work Orders</h2>
      <button class="btn btn-primary" id="create-work-order-btn" @click="navigateToCreate">
        <span class="btn-icon">+</span> New Work Order
      </button>
    </div>
    
    <div class="card">
      <div class="filters">
        <div class="filter-group">
          <label for="status-filter" class="filter-label">Status</label>
          <select id="status-filter" class="form-control" v-model="filters.status">
            <option value="">All Statuses</option>
            <option value="Pending">Pending</option>
            <option value="In Progress">In Progress</option>
            <option value="Completed">Completed</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>
        
        <div class="filter-group">
          <label for="device-filter" class="filter-label">Device Type</label>
          <select id="device-filter" class="form-control" v-model="filters.deviceType">
            <option value="">All Devices</option>
            <option value="Laptop">Laptop</option>
            <option value="Desktop">Desktop</option>
            <option value="Smartphone">Smartphone</option>
            <option value="Tablet">Tablet</option>
            <option value="Printer">Printer</option>
            <option value="Monitor">Monitor</option>
            <option value="Other">Other</option>
          </select>
        </div>
        
        <div class="filter-group">
          <label for="search-filter" class="filter-label">Search</label>
          <input 
            type="text" 
            id="search-filter" 
            class="form-control" 
            placeholder="Search by ID or customer..." 
            v-model="filters.search"
          />
        </div>
      </div>
      
      <table class="work-orders-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Customer</th>
            <th>Device</th>
            <th>Issue</th>
            <th>Status</th>
            <th>Priority</th>
            <th>Created</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="order in filteredWorkOrders" :key="order.id">
            <td>{{ order.id }}</td>
            <td>{{ order.customer }}</td>
            <td>{{ order.deviceType }}</td>
            <td class="issue-cell">{{ order.issue }}</td>
            <td>
              <span class="status-badge" :class="'status-' + order.status.toLowerCase().replace(' ', '-')">
                {{ order.status }}
              </span>
            </td>
            <td>
              <span class="priority-badge" :class="'priority-' + order.priority.toLowerCase()">
                {{ order.priority }}
              </span>
            </td>
            <td>{{ order.created }}</td>
            <td>
              <div class="action-buttons">
                <button class="action-btn view-btn" title="View Details">
                  <span class="action-icon">👁️</span>
                </button>
                <button class="action-btn edit-btn" title="Edit">
                  <span class="action-icon">✏️</span>
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
      
      <div class="pagination">
        <button class="pagination-btn" :disabled="currentPage === 1" @click="currentPage--">
          Previous
        </button>
        <span class="pagination-info">Page {{ currentPage }} of {{ totalPages }}</span>
        <button class="pagination-btn" :disabled="currentPage === totalPages" @click="currentPage++">
          Next
        </button>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'WorkOrderList',
  data() {
    return {
      filters: {
        status: '',
        deviceType: '',
        search: ''
      },
      currentPage: 1,
      itemsPerPage: 10,
      workOrders: [
        {
          id: 'WO-1001',
          customer: 'John Smith',
          deviceType: 'Laptop',
          issue: 'Screen not turning on, possible backlight issue',
          status: 'In Progress',
          priority: 'Medium',
          created: '2025-05-12'
        },
        {
          id: 'WO-1002',
          customer: 'Jane Doe',
          deviceType: 'Smartphone',
          issue: 'Battery draining quickly after recent drop',
          status: 'Completed',
          priority: 'Low',
          created: '2025-05-11'
        },
        {
          id: 'WO-1003',
          customer: 'Bob Johnson',
          deviceType: 'Tablet',
          issue: 'Cracked screen needs replacement',
          status: 'Pending',
          priority: 'High',
          created: '2025-05-10'
        },
        {
          id: 'WO-1004',
          customer: 'Alice Brown',
          deviceType: 'Desktop',
          issue: 'Computer randomly shuts down during use',
          status: 'In Progress',
          priority: 'Medium',
          created: '2025-05-09'
        },
        {
          id: 'WO-1005',
          customer: 'Charlie Wilson',
          deviceType: 'Printer',
          issue: 'Paper jam, unable to print documents',
          status: 'Completed',
          priority: 'Low',
          created: '2025-05-08'
        },
        {
          id: 'WO-1006',
          customer: 'Diana Martinez',
          deviceType: 'Monitor',
          issue: 'Flickering display, possible connection issue',
          status: 'Pending',
          priority: 'Medium',
          created: '2025-05-07'
        },
        {
          id: 'WO-1007',
          customer: 'Edward Lee',
          deviceType: 'Laptop',
          issue: 'Keyboard not responding to certain keys',
          status: 'In Progress',
          priority: 'Low',
          created: '2025-05-06'
        },
        {
          id: 'WO-1008',
          customer: 'Fiona Clark',
          deviceType: 'Smartphone',
          issue: 'Water damage, not powering on',
          status: 'Pending',
          priority: 'Urgent',
          created: '2025-05-05'
        },
        {
          id: 'WO-1009',
          customer: 'George Rodriguez',
          deviceType: 'Desktop',
          issue: 'Loud fan noise, possible overheating',
          status: 'In Progress',
          priority: 'High',
          created: '2025-05-04'
        },
        {
          id: 'WO-1010',
          customer: 'Hannah Kim',
          deviceType: 'Tablet',
          issue: 'Not charging when plugged in',
          status: 'Completed',
          priority: 'Medium',
          created: '2025-05-03'
        },
        {
          id: 'WO-1011',
          customer: 'Ian Patel',
          deviceType: 'Printer',
          issue: 'Print quality issues, streaks on output',
          status: 'Cancelled',
          priority: 'Low',
          created: '2025-05-02'
        },
        {
          id: 'WO-1012',
          customer: 'Julia Thompson',
          deviceType: 'Laptop',
          issue: 'Blue screen errors when running certain applications',
          status: 'In Progress',
          priority: 'High',
          created: '2025-05-01'
        }
      ]
    };
  },
  computed: {
    filteredWorkOrders() {
      let filtered = [...this.workOrders];
      
      // Apply status filter
      if (this.filters.status) {
        filtered = filtered.filter(order => order.status === this.filters.status);
      }
      
      // Apply device type filter
      if (this.filters.deviceType) {
        filtered = filtered.filter(order => order.deviceType === this.filters.deviceType);
      }
      
      // Apply search filter
      if (this.filters.search) {
        const searchTerm = this.filters.search.toLowerCase();
        filtered = filtered.filter(order => 
          order.id.toLowerCase().includes(searchTerm) || 
          order.customer.toLowerCase().includes(searchTerm)
        );
      }
      
      // Calculate pagination
      const startIndex = (this.currentPage - 1) * this.itemsPerPage;
      const endIndex = startIndex + this.itemsPerPage;
      
      return filtered.slice(startIndex, endIndex);
    },
    totalPages() {
      // Calculate total pages based on filtered results
      let filtered = [...this.workOrders];
      
      if (this.filters.status) {
        filtered = filtered.filter(order => order.status === this.filters.status);
      }
      
      if (this.filters.deviceType) {
        filtered = filtered.filter(order => order.deviceType === this.filters.deviceType);
      }
      
      if (this.filters.search) {
        const searchTerm = this.filters.search.toLowerCase();
        filtered = filtered.filter(order => 
          order.id.toLowerCase().includes(searchTerm) || 
          order.customer.toLowerCase().includes(searchTerm)
        );
      }
      
      return Math.ceil(filtered.length / this.itemsPerPage);
    }
  },
  methods: {
    navigateToCreate() {
      this.$router.push('/workorders/new');
    }
  },
  watch: {
    // Reset to first page when filters change
    'filters': {
      handler() {
        this.currentPage = 1;
      },
      deep: true
    }
  }
};
</script>

<style scoped>
.work-order-list {
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

.filters {
  display: flex;
  gap: 16px;
  margin-bottom: 24px;
  flex-wrap: wrap;
}

.filter-group {
  flex: 1;
  min-width: 200px;
}

.filter-label {
  display: block;
  margin-bottom: 8px;
  font-weight: 500;
}

.work-orders-table {
  width: 100%;
  border-collapse: collapse;
}

.work-orders-table th,
.work-orders-table td {
  padding: 12px 16px;
  text-align: left;
  border-bottom: 1px solid var(--border-color);
}

.work-orders-table th {
  font-weight: 600;
  color: #6c757d;
}

.issue-cell {
  max-width: 250px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.status-badge,
.priority-badge {
  display: inline-block;
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
}

.status-completed {
  background-color: #d1e7dd;
  color: #0f5132;
}

.status-in-progress {
  background-color: #cfe2ff;
  color: #084298;
}

.status-pending {
  background-color: #fff3cd;
  color: #664d03;
}

.status-cancelled {
  background-color: #f8d7da;
  color: #842029;
}

.priority-urgent {
  background-color: #f8d7da;
  color: #842029;
}

.priority-high {
  background-color: #ffe5d0;
  color: #7f4012;
}

.priority-medium {
  background-color: #fff3cd;
  color: #664d03;
}

.priority-low {
  background-color: #d1e7dd;
  color: #0f5132;
}

.action-buttons {
  display: flex;
  gap: 8px;
}

.action-btn {
  width: 28px;
  height: 28px;
  border-radius: 4px;
  border: 1px solid var(--border-color);
  background-color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
}

.action-btn:hover {
  background-color: var(--secondary-color);
}

.action-icon {
  font-size: 14px;
}

.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  margin-top: 24px;
  gap: 16px;
}

.pagination-btn {
  padding: 6px 12px;
  border: 1px solid var(--border-color);
  border-radius: 4px;
  background-color: white;
  cursor: pointer;
}

.pagination-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.pagination-info {
  font-size: 14px;
  color: #6c757d;
}
</style>
