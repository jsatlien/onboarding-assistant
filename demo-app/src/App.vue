<template>
  <div class="app">
    <header class="app-header">
      <div class="logo">
        <img src="./assets/logo.svg" alt="Repair Manager" />
        <h1>Repair Manager</h1>
      </div>
      <nav class="main-nav">
        <router-link to="/dashboard">Dashboard</router-link>
        <router-link to="/workorders">Work Orders</router-link>
        <router-link to="/repair-programs">Repair Programs</router-link>
      </nav>
      <div class="user-menu">
        <span class="user-name">John Doe</span>
        <img src="./assets/avatar.svg" alt="User" class="user-avatar" />
      </div>
    </header>
    
    <main class="app-content">
      <router-view />
    </main>
    
    <!-- Onboarding Assistant Widget -->
    <AssistantWidget 
      api-base-url="http://localhost:5000/api"
      assistant-name="Repair Assistant"
    />
  </div>
</template>

<script>
export default {
  name: 'App'
}
</script>

<style>
:root {
  --primary-color: #4a6cf7;
  --secondary-color: #f8f9fa;
  --text-color: #212529;
  --border-color: #dee2e6;
}

* {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
  color: var(--text-color);
  background-color: #f5f7fb;
  line-height: 1.6;
}

.app {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

.app-header {
  display: flex;
  align-items: center;
  padding: 0 24px;
  height: 64px;
  background-color: white;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.logo {
  display: flex;
  align-items: center;
}

.logo img {
  height: 32px;
  margin-right: 12px;
}

.logo h1 {
  font-size: 18px;
  font-weight: 600;
}

.main-nav {
  flex: 1;
  display: flex;
  margin-left: 48px;
}

.main-nav a {
  color: var(--text-color);
  text-decoration: none;
  padding: 8px 16px;
  border-radius: 4px;
  font-weight: 500;
}

.main-nav a:hover {
  background-color: var(--secondary-color);
}

.main-nav a.router-link-active {
  color: var(--primary-color);
  font-weight: 600;
}

.user-menu {
  display: flex;
  align-items: center;
}

.user-name {
  margin-right: 12px;
  font-weight: 500;
}

.user-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
}

.app-content {
  flex: 1;
  padding: 24px;
}

/* Common components */
.card {
  background-color: white;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
  padding: 24px;
  margin-bottom: 24px;
}

.card-title {
  font-size: 18px;
  font-weight: 600;
  margin-bottom: 16px;
}

.btn {
  display: inline-block;
  padding: 8px 16px;
  border-radius: 4px;
  font-weight: 500;
  cursor: pointer;
  border: none;
  text-align: center;
}

.btn-primary {
  background-color: var(--primary-color);
  color: white;
}

.btn-primary:hover {
  background-color: #3a5ce5;
}

.btn-secondary {
  background-color: var(--secondary-color);
  color: var(--text-color);
  border: 1px solid var(--border-color);
}

.btn-secondary:hover {
  background-color: #e9ecef;
}

.form-group {
  margin-bottom: 16px;
}

.form-label {
  display: block;
  margin-bottom: 8px;
  font-weight: 500;
}

.form-control {
  width: 100%;
  padding: 8px 12px;
  border: 1px solid var(--border-color);
  border-radius: 4px;
  font-size: 14px;
}

.form-control:focus {
  outline: none;
  border-color: var(--primary-color);
}
</style>
