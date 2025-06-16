// Copyright (c) Microsoft. All rights reserved.

import {
    Button,
    Field,
    Input,
    Label,
    MessageBar,
    MessageBarBody,
    MessageBarTitle,
    Spinner,
    Switch,
    Table,
    TableBody,
    TableCell,
    TableCellLayout,
    TableColumnDefinition,
    TableHeader,
    TableHeaderCell,
    TableRow,
    createTableColumn,
    makeStyles,
    shorthands,
    tokens,
} from '@fluentui/react-components';
import { AddRegular, DeleteRegular, LinkRegular } from '@fluentui/react-icons';
import React, { useState } from 'react';
import { TabView } from './TabView';

const useClasses = makeStyles({
    container: {
        display: 'flex',
        flexDirection: 'column',
        ...shorthands.gap(tokens.spacingVerticalM),
    },
    section: {
        ...shorthands.padding(tokens.spacingVerticalM),
        ...shorthands.border('1px', 'solid', tokens.colorNeutralStroke2),
        borderRadius: tokens.borderRadiusMedium,
        backgroundColor: tokens.colorNeutralBackground1,
    },
    configForm: {
        display: 'flex',
        flexDirection: 'column',
        ...shorthands.gap(tokens.spacingVerticalS),
    },
    actionButtons: {
        display: 'flex',
        ...shorthands.gap(tokens.spacingHorizontalS),
        justifyContent: 'flex-end',
    },
    table: {
        backgroundColor: tokens.colorNeutralBackground1,
    },
    statusIndicator: {
        display: 'flex',
        alignItems: 'center',
        ...shorthands.gap(tokens.spacingHorizontalXS),
    },
    statusDot: {
        width: '8px',
        height: '8px',
        borderRadius: '50%',
    },
    connected: {
        backgroundColor: tokens.colorPaletteGreenBackground3,
    },
    disconnected: {
        backgroundColor: tokens.colorPaletteRedBackground3,
    },
});

interface MCPConnection {
    id: string;
    name: string;
    uri: string;
    status: 'connected' | 'disconnected' | 'connecting';
    type: 'server' | 'client';
    lastConnected?: Date;
}

interface MCPMessage {
    id: string;
    timestamp: Date;
    type: 'request' | 'response' | 'notification';
    method: string;
    content: any;
    direction: 'inbound' | 'outbound';
}

export const MCPTab: React.FC = () => {
    const classes = useClasses();
    
    const [connections, setConnections] = useState<MCPConnection[]>([
        {
            id: '1',
            name: 'Local MCP Server',
            uri: 'mcp://localhost:3001',
            status: 'disconnected',
            type: 'server',
        },
    ]);
    
    const [messages, setMessages] = useState<MCPMessage[]>([]);
    const [isServerEnabled, setIsServerEnabled] = useState(false);
    const [serverPort, setServerPort] = useState('3000');
    const [isLoading, setIsLoading] = useState(false);
    
    // Form state for new connection
    const [newConnection, setNewConnection] = useState({
        name: '',
        uri: '',
        type: 'client' as 'server' | 'client',
    });

    const { columns: connectionColumns, rows: connectionRows } = useConnectionTable(
        connections,
        (id: string) => {
            void handleConnect(id);
        },
        (id: string) => {
            handleDisconnect(id);
        },
        (id: string) => {
            handleDelete(id);
        }
    );

    const { columns: messageColumns, rows: messageRows } = useMessageTable(messages);

    // Handle connection actions
    async function handleConnect(id: string) {
        setIsLoading(true);
        try {
            const connection = connections.find(c => c.id === id);
            if (!connection) return;

            // Update status to connecting
            setConnections(prev => prev.map(c => 
                c.id === id ? { ...c, status: 'connecting' as const } : c
            ));

            // Simulate connection process
            await new Promise(resolve => setTimeout(resolve, 2000));

            // Update status to connected
            setConnections(prev => prev.map(c => 
                c.id === id ? { 
                    ...c, 
                    status: 'connected' as const, 
                    lastConnected: new Date() 
                } : c
            ));

            // Add a connection message
            const message: MCPMessage = {
                id: Date.now().toString(),
                timestamp: new Date(),
                type: 'notification',
                method: 'initialize',
                content: { status: 'connected', uri: connection.uri },
                direction: 'inbound',
            };
            setMessages(prev => [message, ...prev]);

        } catch (error) {
            console.error('Failed to connect:', error);
            setConnections(prev => prev.map(c => 
                c.id === id ? { ...c, status: 'disconnected' as const } : c
            ));
        } finally {
            setIsLoading(false);
        }
    }

    function handleDisconnect(id: string) {
        setConnections(prev => prev.map(c => 
            c.id === id ? { ...c, status: 'disconnected' as const } : c
        ));

        // Add a disconnection message
        const message: MCPMessage = {
            id: Date.now().toString(),
            timestamp: new Date(),
            type: 'notification',
            method: 'disconnect',
            content: { status: 'disconnected' },
            direction: 'outbound',
        };
        setMessages(prev => [message, ...prev]);
    }

    function handleDelete(id: string) {
        setConnections(prev => prev.filter(c => c.id !== id));
    }

    function handleAddConnection() {
        if (!newConnection.name || !newConnection.uri) return;

        const connection: MCPConnection = {
            id: Date.now().toString(),
            name: newConnection.name,
            uri: newConnection.uri,
            status: 'disconnected',
            type: newConnection.type,
        };

        setConnections(prev => [...prev, connection]);
        setNewConnection({ name: '', uri: '', type: 'client' });
    }

    async function handleToggleServer() {
        setIsLoading(true);
        try {
            if (isServerEnabled) {
                // Stop server
                setIsServerEnabled(false);
                const message: MCPMessage = {
                    id: Date.now().toString(),
                    timestamp: new Date(),
                    type: 'notification',
                    method: 'server_stop',
                    content: { port: serverPort },
                    direction: 'outbound',
                };
                setMessages(prev => [message, ...prev]);
            } else {
                // Start server
                await new Promise(resolve => setTimeout(resolve, 1000));
                setIsServerEnabled(true);
                const message: MCPMessage = {
                    id: Date.now().toString(),
                    timestamp: new Date(),
                    type: 'notification',
                    method: 'server_start',
                    content: { port: serverPort, uri: `mcp://localhost:${serverPort}` },
                    direction: 'outbound',
                };
                setMessages(prev => [message, ...prev]);
            }
        } catch (error) {
            console.error('Failed to toggle server:', error);
        } finally {
            setIsLoading(false);
        }
    }

    return (
        <TabView
            title="MCP (Model Context Protocol)"
            learnMoreDescription="Model Context Protocol"
            learnMoreLink="https://modelcontextprotocol.io/"
        >
            <div className={classes.container}>
                {/* Server Configuration */}
                <div className={classes.section}>
                    <Label size="large" weight="semibold">MCP Server</Label>
                    <div className={classes.configForm}>
                        <MessageBar intent="info">
                            <MessageBarBody>
                                <MessageBarTitle>MCP Server</MessageBarTitle>
                                Enable the MCP server to allow other applications (like GitHub Copilot) to connect and interact with this chat instance.
                            </MessageBarBody>
                        </MessageBar>
                        
                        <Field label="Server Port">
                            <Input
                                value={serverPort}
                                onChange={(_, data) => { setServerPort(data.value); }}
                                disabled={isServerEnabled}
                                placeholder="3000"
                            />
                        </Field>
                        
                        <Field>
                            <Switch
                                checked={isServerEnabled}
                                onChange={() => {
                                    void handleToggleServer();
                                }}
                                label={`MCP Server ${isServerEnabled ? 'Enabled' : 'Disabled'}`}
                                disabled={isLoading}
                            />
                        </Field>
                        
                        {isServerEnabled && (
                            <MessageBar intent="success">
                                <MessageBarBody>
                                    MCP Server is running on mcp://localhost:{serverPort}
                                </MessageBarBody>
                            </MessageBar>
                        )}
                    </div>
                </div>

                {/* Client Connections */}
                <div className={classes.section}>
                    <Label size="large" weight="semibold">MCP Connections</Label>
                    
                    {/* Add New Connection Form */}
                    <div className={classes.configForm}>
                        <Field label="Connection Name">
                            <Input
                                value={newConnection.name}
                                onChange={(_, data) => {
                                    setNewConnection(prev => ({ ...prev, name: data.value }));
                                }}
                                placeholder="e.g., GitHub Copilot"
                            />
                        </Field>
                        
                        <Field label="MCP URI">
                            <Input
                                value={newConnection.uri}
                                onChange={(_, data) => {
                                    setNewConnection(prev => ({ ...prev, uri: data.value }));
                                }}
                                placeholder="mcp://localhost:3001"
                            />
                        </Field>
                        
                        <div className={classes.actionButtons}>
                            <Button
                                icon={<AddRegular />}
                                onClick={handleAddConnection}
                                disabled={!newConnection.name || !newConnection.uri}
                            >
                                Add Connection
                            </Button>
                        </div>
                    </div>

                    {/* Connections Table */}
                    <Table aria-label="MCP Connections" className={classes.table}>
                        <TableHeader>
                            <TableRow>
                                {connectionColumns.map((column) => column.renderHeaderCell())}
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {connectionRows.map((item) => (
                                <TableRow key={item.id}>
                                    {connectionColumns.map((column) => column.renderCell(item))}
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </div>

                {/* Message Log */}
                <div className={classes.section}>
                    <Label size="large" weight="semibold">Message Log</Label>
                    <Table aria-label="MCP Messages" className={classes.table} size="small">
                        <TableHeader>
                            <TableRow>
                                {messageColumns.map((column) => column.renderHeaderCell())}
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {messageRows.map((item) => (
                                <TableRow key={item.id}>
                                    {messageColumns.map((column) => column.renderCell(item))}
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </div>
            </div>
        </TabView>
    );
};

function useConnectionTable(
    connections: MCPConnection[],
    onConnect: (id: string) => void,
    onDisconnect: (id: string) => void,
    onDelete: (id: string) => void
): { columns: Array<TableColumnDefinition<MCPConnection>>; rows: MCPConnection[] } {
    const classes = useClasses();

    const columns: Array<TableColumnDefinition<MCPConnection>> = [
        createTableColumn<MCPConnection>({
            columnId: 'name',
            renderHeaderCell: () => <TableHeaderCell>Name</TableHeaderCell>,
            renderCell: (item) => (
                <TableCell>
                    <TableCellLayout>{item.name}</TableCellLayout>
                </TableCell>
            ),
        }),
        createTableColumn<MCPConnection>({
            columnId: 'uri',
            renderHeaderCell: () => <TableHeaderCell>URI</TableHeaderCell>,
            renderCell: (item) => (
                <TableCell>
                    <TableCellLayout>{item.uri}</TableCellLayout>
                </TableCell>
            ),
        }),
        createTableColumn<MCPConnection>({
            columnId: 'status',
            renderHeaderCell: () => <TableHeaderCell>Status</TableHeaderCell>,
            renderCell: (item) => (
                <TableCell>
                    <TableCellLayout>
                        <div className={classes.statusIndicator}>
                            <div 
                                className={`${classes.statusDot} ${
                                    item.status === 'connected' ? classes.connected : classes.disconnected
                                }`}
                            />
                            {item.status === 'connecting' ? (
                                <>
                                    <Spinner size="tiny" />
                                    <span>Connecting...</span>
                                </>
                            ) : (
                                <span>{item.status}</span>
                            )}
                        </div>
                    </TableCellLayout>
                </TableCell>
            ),
        }),
        createTableColumn<MCPConnection>({
            columnId: 'actions',
            renderHeaderCell: () => <TableHeaderCell>Actions</TableHeaderCell>,
            renderCell: (item) => (
                <TableCell>
                    <TableCellLayout>
                        <div className={classes.actionButtons}>
                            {item.status === 'connected' ? (
                                <Button
                                    size="small"
                                    onClick={() => {
                                        onDisconnect(item.id);
                                    }}
                                >
                                    Disconnect
                                </Button>
                            ) : (
                                <Button
                                    size="small"
                                    icon={<LinkRegular />}
                                    onClick={() => {
                                        onConnect(item.id);
                                    }}
                                    disabled={item.status === 'connecting'}
                                >
                                    Connect
                                </Button>
                            )}
                            <Button
                                size="small"
                                icon={<DeleteRegular />}
                                onClick={() => {
                                    onDelete(item.id);
                                }}
                                appearance="subtle"
                            >
                                Delete
                            </Button>
                        </div>
                    </TableCellLayout>
                </TableCell>
            ),
        }),
    ];

    return { columns, rows: connections };
}

function useMessageTable(messages: MCPMessage[]): { columns: Array<TableColumnDefinition<MCPMessage>>; rows: MCPMessage[] } {
    const columns: Array<TableColumnDefinition<MCPMessage>> = [
        createTableColumn<MCPMessage>({
            columnId: 'timestamp',
            renderHeaderCell: () => <TableHeaderCell>Time</TableHeaderCell>,
            renderCell: (item) => (
                <TableCell>
                    <TableCellLayout>
                        {item.timestamp.toLocaleTimeString()}
                    </TableCellLayout>
                </TableCell>
            ),
        }),
        createTableColumn<MCPMessage>({
            columnId: 'direction',
            renderHeaderCell: () => <TableHeaderCell>Direction</TableHeaderCell>,
            renderCell: (item) => (
                <TableCell>
                    <TableCellLayout>
                        {item.direction === 'inbound' ? '→' : '←'} {item.direction}
                    </TableCellLayout>
                </TableCell>
            ),
        }),
        createTableColumn<MCPMessage>({
            columnId: 'type',
            renderHeaderCell: () => <TableHeaderCell>Type</TableHeaderCell>,
            renderCell: (item) => (
                <TableCell>
                    <TableCellLayout>{item.type}</TableCellLayout>
                </TableCell>
            ),
        }),
        createTableColumn<MCPMessage>({
            columnId: 'method',
            renderHeaderCell: () => <TableHeaderCell>Method</TableHeaderCell>,
            renderCell: (item) => (
                <TableCell>
                    <TableCellLayout>{item.method}</TableCellLayout>
                </TableCell>
            ),
        }),
        createTableColumn<MCPMessage>({
            columnId: 'content',
            renderHeaderCell: () => <TableHeaderCell>Content</TableHeaderCell>,
            renderCell: (item) => (
                <TableCell>
                    <TableCellLayout>
                        <pre style={{ fontSize: '0.8em', margin: 0 }}>
                            {JSON.stringify(item.content, null, 2)}
                        </pre>
                    </TableCellLayout>
                </TableCell>
            ),
        }),
    ];

    return { columns, rows: messages };
}