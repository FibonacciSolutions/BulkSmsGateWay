import React, { useState, useEffect } from 'react';
import { 
  LayoutDashboard, 
  MessageSquare, 
  Smartphone, 
  Wallet, 
  RefreshCw, 
  CheckCircle2, 
  AlertCircle, 
  Layers 
} from 'lucide-react';

export default function App() {
  // Update this to match your local running .NET Core API port (e.g., 5001 or 7123)
  const API_BASE_URL = "http://localhost:5235/api/v1"; 
  const DEV_API_KEY = "omni_live_devtestkey1234567890abcdef";

  const [tenantInfo, setTenantInfo] = useState({
    name: "Loading Scope...",
    balance: "0.00",
    currency: "NPR",
    apiKey: DEV_API_KEY
  });

  const [logs, setLogs] = useState([]);
  const [isLoading, setIsLoading] = useState(false);

  // Function to pull live system data from the .NET Core Engine
  const fetchDashboardData = async () => {
  setIsLoading(true);
  
  // 1. Fetch current tenant balance status safely
  try {
    const tenantRes = await fetch(`${API_BASE_URL}/tenants/profile`, {
      headers: { 'X-API-KEY': DEV_API_KEY }
    });
    if (tenantRes.ok) {
      const tenantData = await tenantRes.json();
      setTenantInfo({
        name: tenantData.companyName || "Apex International Academy",
        balance: tenantData.walletBalance || "500.00",
        currency: "NPR",
        apiKey: DEV_API_KEY
      });
    } else {
      console.warn("Tenant profile endpoint returned a non-200 status. Using fallback data.");
    }
  } catch (tenantError) {
    console.error("Failed to fetch tenant profile context:", tenantError);
  }

  // 2. Fetch live asynchronous routing logs independently 🚀
  try {
    const logsRes = await fetch(`${API_BASE_URL}/messages/logs`, {
      headers: { 'X-API-KEY': DEV_API_KEY }
    });
    if (logsRes.ok) {
      const logsData = await logsRes.json();
      console.log("Successfully fetched logs data payload:", logsData);
      setLogs(logsData); // This will execute perfectly now!
    } else {
      console.error("Logs endpoint failed with status:", logsRes.status);
    }
  } catch (logsError) {
    console.error("Failed to connect to OmniRoute.Api message log engine:", logsError);
  } finally {
    setIsLoading(false);
  }
};

  // Trigger data pool on application mount
  useEffect(() => {
    fetchDashboardData();
  }, []);

  return (
    <div className="flex h-screen w-screen bg-slate-50 text-slate-800 overflow-hidden">
      
      {/* SIDEBAR */}
      <aside className="w-64 bg-slate-900 text-slate-300 flex flex-col justify-between border-r border-slate-800 shrink-0">
        <div>
          <div className="p-6 flex items-center gap-3 border-b border-slate-800">
            <div className="bg-indigo-600 text-white p-2 rounded-lg">
              <Layers size={20} />
            </div>
            <div>
              <h1 className="font-bold text-white text-lg tracking-tight">OmniRoute</h1>
              <span className="text-xs text-slate-500 font-medium">Enterprise Engine</span>
            </div>
          </div>

          <nav className="p-4 space-y-1">
            <a href="#" className="flex items-center gap-3 px-4 py-3 bg-indigo-600 text-white rounded-lg font-medium">
              <LayoutDashboard size={18} />
              Dashboard
            </a>
            <a href="#" className="flex items-center gap-3 px-4 py-3 text-slate-400 hover:bg-slate-800 hover:text-white rounded-lg font-medium">
              <MessageSquare size={18} />
              Message Logs
            </a>
            <a href="#" className="flex items-center gap-3 px-4 py-3 text-slate-400 hover:bg-slate-800 hover:text-white rounded-lg font-medium">
              <Smartphone size={18} />
              Route Templates
            </a>
            <a href="#" className="flex items-center gap-3 px-4 py-3 text-slate-400 hover:bg-slate-800 hover:text-white rounded-lg font-medium">
              <Wallet size={18} />
              Billing & Wallet
            </a>
          </nav>
        </div>

        <div className="p-4 border-t border-slate-800 bg-slate-950">
          <p className="text-xs text-slate-500 uppercase tracking-wider font-bold">Active Scope</p>
          <p className="text-sm font-semibold text-slate-200 truncate">{tenantInfo.name}</p>
        </div>
      </aside>

      {/* MAIN WORKSPACE */}
      <main className="flex-1 flex flex-col overflow-y-auto">
        <header className="h-16 bg-white border-b border-slate-200 flex items-center justify-between px-8 shrink-0">
          <span className="text-xs font-mono bg-slate-100 text-slate-600 px-3 py-1 rounded-full border border-slate-200">
            Token: {tenantInfo.apiKey.substring(0, 13)}...
          </span>
          <button 
            onClick={fetchDashboardData}
            disabled={isLoading}
            className="flex items-center gap-2 text-sm font-medium bg-slate-100 hover:bg-slate-200 disabled:opacity-50 px-4 py-2 rounded-lg text-slate-600 transition"
          >
            <RefreshCw size={14} className={isLoading ? "animate-spin" : ""} />
            {isLoading ? "Syncing..." : "Refresh Data"}
          </button>
        </header>

        <div className="p-8 space-y-8 max-w-7xl w-full mx-auto">
          <div>
            <h2 className="text-2xl font-bold text-slate-900 tracking-tight">System Performance Analytics</h2>
            <p className="text-sm text-slate-500">Monitor multi-channel delivery funnels, real-time message routing, and utility balances.</p>
          </div>

          {/* CARDS */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-sm flex items-center justify-between">
              <div className="space-y-1">
                <p className="text-xs font-semibold text-slate-400 uppercase tracking-wider">Available Utility Balance</p>
                <p className="text-3xl font-bold text-slate-900">{tenantInfo.balance} <span className="text-sm font-semibold text-slate-500">{tenantInfo.currency}</span></p>
              </div>
              <div className="bg-emerald-50 text-emerald-600 p-4 rounded-xl">
                <Wallet size={24} />
              </div>
            </div>

            <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-sm flex items-center justify-between">
              <div className="space-y-1">
                <p className="text-xs font-semibold text-slate-400 uppercase tracking-wider">WhatsApp Dispatches</p>
                <p className="text-3xl font-bold text-slate-900">{logs.filter(l => l.channel === 'WhatsApp').length} <span className="text-xs font-medium text-emerald-500 font-mono ml-1">Live</span></p>
              </div>
              <div className="bg-indigo-50 text-indigo-600 p-4 rounded-xl">
                <MessageSquare size={24} />
              </div>
            </div>

            <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-sm flex items-center justify-between">
              <div className="space-y-1">
                <p className="text-xs font-semibold text-slate-400 uppercase tracking-wider">Fallback SMS Dispatches</p>
                <p className="text-3xl font-bold text-slate-900">{logs.filter(l => l.channel === 'SMS').length} <span className="text-xs font-medium text-slate-400 font-mono ml-1">Routed</span></p>
              </div>
              <div className="bg-sky-50 text-sky-600 p-4 rounded-xl">
                <Smartphone size={24} />
              </div>
            </div>
          </div>

          {/* TABLE */}
          <div className="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
            <div className="p-6 border-b border-slate-200 bg-slate-50/50 flex items-center justify-between">
              <div>
                <h3 className="font-bold text-slate-900">Live Real-Time Routing Ledger</h3>
                <p className="text-xs text-slate-500">Asynchronous processing pipeline monitor logs from core routing node context.</p>
              </div>
              <span className="flex items-center gap-1.5 text-xs font-semibold bg-emerald-50 text-emerald-700 px-2.5 py-1 rounded-full border border-emerald-200 animate-pulse">
                <span className="w-2 h-2 rounded-full bg-emerald-500"></span>
                Engine Listening
              </span>
            </div>

            <div className="overflow-x-auto">
              <table className="w-full text-left border-collapse">
                <thead>
                  <tr className="bg-slate-50 border-b border-slate-200 text-xs font-semibold uppercase tracking-wider text-slate-400">
                    <th className="py-3 px-6">Destination Number</th>
                    <th className="py-3 px-6">Active Route</th>
                    <th className="py-3 px-6">Target Template</th>
                    <th className="py-3 px-6">Pipeline Status</th>
                    <th className="py-3 px-6">Billing Charge</th>
                    <th className="py-3 px-6">Timestamp</th>
                  </tr>
                </thead>
                <tbody className="text-sm divide-y divide-slate-200 font-medium">
                  {logs.length === 0 ? (
                    <tr>
                      <td colSpan="6" className="py-8 text-center text-slate-400 font-normal">No transactional routing logs synced yet. Run a message via backend console to view.</td>
                    </tr>
                  ) : (
                    logs.map((log) => (
                      <tr key={log.id} className="hover:bg-slate-50/70 transition">
                        <td className="py-4 px-6 font-mono font-semibold text-slate-900">{log.recipientNumber || log.recipient}</td>
                        <td className="py-4 px-6">
                          <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md text-xs font-bold border ${
                            log.channel === 'WhatsApp' 
                              ? 'bg-indigo-50 text-indigo-700 border-indigo-200' 
                              : 'bg-sky-50 text-sky-700 border-sky-200'
                          }`}>
                            {log.channel}
                          </span>
                        </td>
                        <td className="py-4 px-6 font-mono text-xs text-slate-500">{log.templateCode || log.template}</td>
                        <td className="py-4 px-6">
                          <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded text-xs font-semibold ${
                            log.deliveryStatus === 'Read' || log.deliveryStatus === 'Delivered' ? 'bg-emerald-50 text-emerald-700' :
                            log.deliveryStatus === 'Dispatched' || log.deliveryStatus === 'Sent' ? 'bg-amber-50 text-amber-700' :
                            'bg-rose-50 text-rose-700'
                          }`}>
                            {log.deliveryStatus}
                          </span>
                        </td>
                        <td className="py-4 px-6 font-mono text-slate-700">-{log.costPerMessage || log.cost} NPR</td>
                        <td className="py-4 px-6 text-slate-400 text-xs font-normal">
                          {log.createdAt ? new Date(log.createdAt).toLocaleTimeString() : "Just Now"}
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </div>

        </div>
      </main>
    </div>
  );
}