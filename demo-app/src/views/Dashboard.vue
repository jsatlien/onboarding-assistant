<template>
  <div class="dashboard">
    <h2 class="page-title">Dashboard</h2>
    
    <div class="dashboard-grid">
      <div class="card stats-card">
        <h3 class="card-title">Repair Statistics</h3>
        <div class="stats-chart" id="repair-stats-chart">
          <div class="chart-placeholder">
            <div class="chart-bar" style="height: 60%;">
              <span class="chart-label">Mon</span>
            </div>
            <div class="chart-bar" style="height: 80%;">
              <span class="chart-label">Tue</span>
            </div>
            <div class="chart-bar" style="height: 40%;">
              <span class="chart-label">Wed</span>
            </div>
            <div class="chart-bar" style="height: 70%;">
              <span class="chart-label">Thu</span>
            </div>
            <div class="chart-bar" style="height: 90%;">
              <span class="chart-label">Fri</span>
            </div>
          </div>
          <div class="chart-legend">
            <div class="legend-item">
              <span class="legend-color"></span>
              <span>Completed Repairs</span>
            </div>
          </div>
        </div>
      </div>
      
      <div class="card quick-actions-card">
        <h3 class="card-title">Quick Actions</h3>
        <div class="quick-actions-panel" id="quick-actions-panel">
          <button class="action-button" @click="navigateTo('/workorders/new')">
            <span class="action-icon">+</span>
            <span>New Work Order</span>
          </button>
          <button class="action-button">
            <span class="action-icon">👁️</span>
            <span>View Inventory</span>
          </button>
          <button class="action-button">
            <span class="action-icon">📋</span>
            <span>Manage Programs</span>
          </button>
          <button class="action-button">
            <span class="action-icon">👥</span>
            <span>Assign Technician</span>
          </button>
        </div>
      </div>
      
      <div class="card recent-orders-card">
        <h3 class="card-title">Recent Work Orders</h3>
        <div class="recent-work-orders" id="recent-work-orders">
          <table class="orders-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Customer</th>
                <th>Device</th>
                <th>Status</th>
                <th>Created</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="order in recentOrders" :key="order.id">
                <td>{{ order.id }}</td>
                <td>{{ order.customer }}</td>
                <td>{{ order.device }}</td>
                <td>
                  <span class="status-badge" :class="'status-' + order.status.toLowerCase()">
                    {{ order.status }}
                  </span>
                </td>
                <td>{{ order.created }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'Dashboard',
  data() {
    return {
      recentOrders: [
        { id: 'WO-1001', customer: 'John Smith', device: 'Laptop', status: 'In Progress', created: '2025-05-12' },
        { id: 'WO-1002', customer: 'Jane Doe', device: 'Smartphone', status: 'Completed', created: '2025-05-11' },
        { id: 'WO-1003', customer: 'Bob Johnson', device: 'Tablet', status: 'Pending', created: '2025-05-10' },
        { id: 'WO-1004', customer: 'Alice Brown', device: 'Desktop', status: 'In Progress', created: '2025-05-09' },
        { id: 'WO-1005', customer: 'Charlie Wilson', device: 'Printer', status: 'Completed', created: '2025-05-08' }
      ]
    };
  },
  methods: {
    navigateTo(route) {
      this.$router.push(route);
    }
  }
};
</script>

<style scoped>
.dashboard-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  grid-template-rows: auto 1fr;
  gap: 24px;
}

.stats-card {
  grid-column: 1 / 2;
}

.quick-actions-card {
  grid-column: 2 / 3;
}

.recent-orders-card {
  grid-column: 1 / 3;
}

.page-title {
  margin-bottom: 24px;
  font-size: 24px;
  font-weight: 600;
}

/* Stats Chart */
.stats-chart {
  height: 200px;
}

.chart-placeholder {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  height: 160px;
  margin-bottom: 16px;
}

.chart-bar {
  width: 40px;
  background-color: var(--primary-color);
  border-radius: 4px 4px 0 0;
  position: relative;
  display: flex;
  justify-content: center;
}

.chart-label {
  position: absolute;
  bottom: -24px;
  font-size: 12px;
  color: #6c757d;
}

.chart-legend {
  display: flex;
  justify-content: center;
  margin-top: 32px;
}

.legend-item {
  display: flex;
  align-items: center;
  margin-right: 16px;
}

.legend-color {
  display: inline-block;
  width: 12px;
  height: 12px;
  margin-right: 8px;
  background-color: var(--primary-color);
  border-radius: 2px;
}

/* Quick Actions */
.quick-actions-panel {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
}

.action-button {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 16px;
  background-color: var(--secondary-color);
  border: 1px solid var(--border-color);
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
}

.action-button:hover {
  background-color: #e9ecef;
  transform: translateY(-2px);
}

.action-icon {
  font-size: 24px;
  margin-bottom: 8px;
}

/* Recent Work Orders */
.orders-table {
  width: 100%;
  border-collapse: collapse;
}

.orders-table th,
.orders-table td {
  padding: 12px 16px;
  text-align: left;
  border-bottom: 1px solid var(--border-color);
}

.orders-table th {
  font-weight: 600;
  color: #6c757d;
}

.status-badge {
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

.status-in.progress {
  background-color: #cfe2ff;
  color: #084298;
}

.status-pending {
  background-color: #fff3cd;
  color: #664d03;
}
</style>
