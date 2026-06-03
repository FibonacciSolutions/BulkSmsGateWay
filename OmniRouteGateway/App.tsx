import React, { useState, useEffect } from 'react';
import { StyleSheet, Text, View, TouchableOpacity, SafeAreaView, PermissionsAndroid } from 'react-native';
import SmsAndroid from 'react-native-get-sms-android';

export default function App() {
  const [isRunning, setIsRunning] = useState(false);
  const [logStatus, setLogStatus] = useState('Gateway Offline. Press Start to bind system.');
  const [totalDispatched, setTotalDispatched] = useState(0);

  // 🛠️ DEVELOPER CONFIGURATION: Enter your laptop's current network IP address
  const apiEndpoint = '192.168.1.72';

  // Request native hardware access keys from Android on boot
  const requestSmsPermission = async () => {
    try {
      await PermissionsAndroid.requestMultiple([
        PermissionsAndroid.PERMISSIONS.SEND_SMS,
        PermissionsAndroid.PERMISSIONS.READ_PHONE_STATE,
      ]);
    } catch (err) {
      console.warn(err);
    }
  };

  useEffect(() => {
    requestSmsPermission();
  }, []);

  // Main Polling Engine Loop Execution block
  useEffect(() => {
    let timerId: any;

    if (isRunning) {
      setLogStatus('Engine active. Sniffing SQL pending table rows...');
      
      timerId = setInterval(async () => {
        try {
          const response = await fetch(apiEndpoint);
          const data = await response.json();

          if (data.sms_available === true) {
            const { to, message } = data;
            setLogStatus(`Pending task intercepted! Routing to: ${to}`);

            // 🚀 Fire message out through physical device hardware SIM tray
            SmsAndroid.sms(
              to,
              message,
              'sms',
              (err: any) => {
                setLogStatus(`Hardware Error: ${err}`);
              },
              (success: any) => {
                setTotalDispatched((prev) => prev + 1);
                setLogStatus(`Success! Dispatched text to ${to} via cellular SIM.`);
              }
            );
          } else {
            setLogStatus('Active listening loop... Outbox table is clean.');
          }
        } catch (error) {
          setLogStatus('Network Timeout. Cannot locate .NET host server.');
        }
      }, 5000); // Check every 5 seconds
    } else {
      setLogStatus('Engine Stopped. Queue parsing offline.');
    }

    return () => clearInterval(timerId);
  }, [isRunning]);

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.card}>
        <Text style={styles.title}>Octapulse SMS Node</Text>
        <Text style={[styles.status, { color: isRunning ? '#10B981' : '#EF4444' }]}>
          {isRunning ? 'GATEWAY NODE RUNNING' : 'GATEWAY NODE ASLEEP'}
        </Text>

        <View style={styles.logBox}>
          <Text style={styles.logTitle}>SYSTEM LIVE LOG</Text>
          <Text style={styles.logText}>{logStatus}</Text>
        </View>

        <Text style={styles.counter}>
          Successful Airtime Dispatches: <Text style={{ fontWeight: 'bold' }}>{totalDispatched}</Text>
        </Text>

        <TouchableOpacity
          style={[styles.button, { backgroundColor: isRunning ? '#EF4444' : '#10B981' }]}
          onPress={() => setIsRunning(!isRunning)}
        >
          <Text style={styles.buttonText}>
            {isRunning ? 'DEACTIVATE MODULE' : 'ACTIVATE SERVICE MODULE'}
          </Text>
        </TouchableOpacity>
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#F5F7FA', justifyContent: 'center', alignItems: 'center' },
  card: { width: '90%', backgroundColor: '#FFF', padding: 24, borderRadius: 16, shadowColor: '#000', shadowOffset: { width: 0, height: 2 }, shadowOpacity: 0.1, shadowRadius: 4, elevation: 4 },
  title: { fontSize: 22, fontWeight: 'bold', color: '#0F172A', textAlign: 'center', marginBottom: 4 },
  status: { fontSize: 14, fontWeight: '700', textAlign: 'center', marginBottom: 24, letterSpacing: 1 },
  logBox: { backgroundColor: '#F8FAFC', padding: 16, borderRadius: 12, borderWidth: 1, borderColor: '#E2E8F0', marginBottom: 16 },
  logTitle: { fontSize: 10, fontWeight: 'bold', color: '#94A3B8', textAlign: 'center', marginBottom: 8 },
  logText: { fontSize: 14, color: '#334155', textAlign: 'center' },
  counter: { fontSize: 15, color: '#475569', textAlign: 'center', marginBottom: 32 },
  button: { height: 52, borderRadius: 12, justifyContent: 'center', alignItems: 'center' },
  buttonText: { color: '#FFF', fontSize: 16, fontWeight: 'bold' },
});