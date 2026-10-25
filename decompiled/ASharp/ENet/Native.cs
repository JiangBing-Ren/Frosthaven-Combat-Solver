using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace ENet;

[SuppressUnmanagedCodeSecurity]
internal static class Native
{
	private const string nativeLibrary = "enet";

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int enet_initialize();

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int enet_initialize_with_callbacks(uint version, ref ENetCallbacks inits);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_deinitialize();

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint enet_linked_version();

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint enet_time_get();

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern ulong enet_crc64(IntPtr buffers, int bufferCount);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int enet_address_set_ip(ref ENetAddress address, string ip);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int enet_address_set_hostname(ref ENetAddress address, string hostName);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int enet_address_get_ip(ref ENetAddress address, StringBuilder ip, IntPtr ipLength);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int enet_address_get_hostname(ref ENetAddress address, StringBuilder hostName, IntPtr nameLength);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr enet_packet_create(byte[] data, IntPtr dataLength, PacketFlags flags);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr enet_packet_create(IntPtr data, IntPtr dataLength, PacketFlags flags);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr enet_packet_create_offset(byte[] data, IntPtr dataLength, IntPtr dataOffset, PacketFlags flags);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr enet_packet_create_offset(IntPtr data, IntPtr dataLength, IntPtr dataOffset, PacketFlags flags);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int enet_packet_check_references(IntPtr packet);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr enet_packet_get_data(IntPtr packet);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr enet_packet_get_user_data(IntPtr packet);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr enet_packet_set_user_data(IntPtr packet, IntPtr userData);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int enet_packet_get_length(IntPtr packet);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_packet_set_free_callback(IntPtr packet, IntPtr callback);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_packet_dispose(IntPtr packet);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr enet_host_create(ref ENetAddress address, IntPtr peerLimit, IntPtr channelLimit, uint incomingBandwidth, uint outgoingBandwidth, int bufferSize);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr enet_host_create(IntPtr address, IntPtr peerLimit, IntPtr channelLimit, uint incomingBandwidth, uint outgoingBandwidth, int bufferSize);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr enet_host_connect(IntPtr host, ref ENetAddress address, IntPtr channelCount, uint data);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_host_broadcast(IntPtr host, byte channelID, IntPtr packet);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_host_broadcast_exclude(IntPtr host, byte channelID, IntPtr packet, IntPtr excludedPeer);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_host_broadcast_selective(IntPtr host, byte channelID, IntPtr packet, IntPtr[] peers, IntPtr peersLength);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int enet_host_service(IntPtr host, out ENetEvent @event, uint timeout);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int enet_host_check_events(IntPtr host, out ENetEvent @event);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_host_channel_limit(IntPtr host, IntPtr channelLimit);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_host_bandwidth_limit(IntPtr host, uint incomingBandwidth, uint outgoingBandwidth);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint enet_host_get_peers_count(IntPtr host);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint enet_host_get_packets_sent(IntPtr host);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint enet_host_get_packets_received(IntPtr host);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint enet_host_get_bytes_sent(IntPtr host);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint enet_host_get_bytes_received(IntPtr host);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_host_set_max_duplicate_peers(IntPtr host, ushort number);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_host_set_intercept_callback(IntPtr host, IntPtr callback);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_host_set_checksum_callback(IntPtr host, IntPtr callback);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_host_flush(IntPtr host);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_host_destroy(IntPtr host);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_host_prevent_connections(IntPtr host, byte state);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_peer_throttle_configure(IntPtr peer, uint interval, uint acceleration, uint deceleration, uint threshold);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint enet_peer_get_id(IntPtr peer);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int enet_peer_get_ip(IntPtr peer, byte[] ip, IntPtr ipLength);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern ushort enet_peer_get_port(IntPtr peer);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint enet_peer_get_mtu(IntPtr peer);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern PeerState enet_peer_get_state(IntPtr peer);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint enet_peer_get_rtt(IntPtr peer);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint enet_peer_get_last_rtt(IntPtr peer);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint enet_peer_get_lastsendtime(IntPtr peer);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern uint enet_peer_get_lastreceivetime(IntPtr peer);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern ulong enet_peer_get_packets_sent(IntPtr peer);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern ulong enet_peer_get_packets_lost(IntPtr peer);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern float enet_peer_get_packets_throttle(IntPtr peer);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern ulong enet_peer_get_bytes_sent(IntPtr peer);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern ulong enet_peer_get_bytes_received(IntPtr peer);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr enet_peer_get_data(IntPtr peer);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_peer_set_data(IntPtr peer, IntPtr data);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern int enet_peer_send(IntPtr peer, byte channelID, IntPtr packet);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern IntPtr enet_peer_receive(IntPtr peer, out byte channelID);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_peer_ping(IntPtr peer);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_peer_ping_interval(IntPtr peer, uint pingInterval);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_peer_timeout(IntPtr peer, uint timeoutLimit, uint timeoutMinimum, uint timeoutMaximum);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_peer_disconnect(IntPtr peer, uint data);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_peer_disconnect_now(IntPtr peer, uint data);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_peer_disconnect_later(IntPtr peer, uint data);

	[DllImport("enet", CallingConvention = CallingConvention.Cdecl)]
	internal static extern void enet_peer_reset(IntPtr peer);
}
